﻿using ToDoApp.Server.Contracts;
using ToDoApp.Server.Helper;
using ToDoApp.Server.Models;
using ToDoApp.Server.Models.Entity;

namespace ToDoApp.Server.Services
{
    public class SubTaskService(
        IBaseRepository<SubTask> subTaskRepo,
        IBaseRepository<Models.Entity.Task> taskRepo,
        ICacheService cacheService,
        IBaseRepository<TaskGroup> taskGroupRepo) : ISubTaskService
    {
        public async Task<ResponseModel> AddSubTaskAsync(AddSubTaskRequestModel model)
        {
            await subTaskRepo.AddAsync(new SubTask
            {
                Title = model.Title,
                Description = model.Description,
                ToDoDate = model.ToDoDate,
                IsStarred = model.IsStarred,
                IsCompleted = false,
                TaskId = model.TaskId,
                CreateDate = DateTime.Now
            });
            return new()
            {
                Data = await subTaskRepo.GetAllAsync(),
                IsSuccess = true,
                Message = "Subtask added successfully!!"
            };
        }

        public async Task<ResponseModel> DeleteAsync(int subTaskId)
        {
            var cacheModel = new UndoDeleteItemCacheModel();
            var subTask = await subTaskRepo.GetByIdAsync(subTaskId);
            if (subTask == null || subTask.IsDeleted)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Subtask not found"
                };
            }

            subTask.IsDeleted = true;
            await subTaskRepo.UpdateAsync(subTask);
            cacheModel.SubTasksId = [subTaskId];
            cacheService.SetData(ConstantVariables.CacheKeyForUndoItems, cacheModel, DateTimeOffset.Now.AddSeconds(3));
            return new()
            {
                IsSuccess = true,
                Message = "Subtask deleted successfully"
            };
        }

        public async Task<ResponseModel> GetAllSubTaskAsync()
        {
            return new()
            {
                Data = await subTaskRepo.QueryAsync().Where(x => !x.IsDeleted).ToList(),
                IsSuccess = true,
            };
        }

        public async Task<ResponseModel> GetSubTaskByIdAsync(int subTaskId)
        {
            var subTask = await subTaskRepo.GetByIdAsync(subTaskId);
            if (subTask == null || subTask.IsDeleted)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Subtask not found"
                };
            }
            return new()
            {
                Data = new
                {
                    subTask.Title,
                    subTask.SubTaskId,
                    subTask.Description,
                    subTask.TaskId,
                    ToDoDate = subTask.ToDoDate != null ? subTask.ToDoDate?.ToString("yyyy-MM-dd") : null
                },
                IsSuccess = true,
            };
        }

        public async Task<ResponseModel> MoveSubTaskToExistingGroupAsync(int subTaskId, int groupId)
        {
            var cacheModel = new UndoSubTaskMovedCacheModel();
            // Check if the subtask exists
            var subTask = await subTaskRepo.GetByIdAsync(subTaskId);
            if (subTask == null || subTask.IsDeleted)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Subtask not found"
                };
            }
            // Check if the group exists
            var group = await taskGroupRepo.GetByIdAsync(groupId);
            if (group == null || group.IsDeleted)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Group not found"
                };
            }
            // Move the subtask to a group
            var taskToAdd = new Models.Entity.Task()
            {
                Title = subTask.Title,
                Description = subTask.Description,
                ToDoDate = subTask.ToDoDate,
                IsStarred = subTask.IsStarred,
                IsCompleted = subTask.IsCompleted,
                TaskGroupId = groupId,
                CreateDate = DateTime.Now
            };
            await taskRepo.AddAsync(taskToAdd);
            cacheModel.TaskId = taskToAdd.TaskId;
            cacheModel.SubTaskId = subTaskId;
            cacheModel.MovedGroupId = groupId;
            cacheModel.IsExistedGroup = true;
            // Delete the subtask from the subtask list
            subTask.IsDeleted = true;
            await subTaskRepo.UpdateAsync(subTask);
            cacheService.SetData(ConstantVariables.CacheKeyForUndoSubTaskMoved, cacheModel, DateTimeOffset.Now.AddSeconds(3));
            return new()
            {
                IsSuccess = true,
                Message = "Subtask moved to existing group successfully!!"
            };
        }

        public async Task<ResponseModel> MoveSubTaskToNewGroup(int subTaskId, AddGroupRequestModel model)
        {
            var cacheModel = new UndoSubTaskMovedCacheModel();
            //check if the subtask exists
            var subTask = await subTaskRepo.GetByIdAsync(subTaskId);
            if (subTask == null || subTask.IsDeleted)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Subtask not found"
                };
            }
            // Create a new group
            var group = new TaskGroup { GroupName = model.GroupName, IsEnableShow = true, SortBy = "My order" };
            await taskGroupRepo.AddAsync(group);

            var taskToAdd = new Models.Entity.Task()
            {
                Title = subTask.Title,
                Description = subTask.Description,
                ToDoDate = subTask.ToDoDate,
                IsStarred = subTask.IsStarred,
                IsCompleted = subTask.IsCompleted,
                TaskGroupId = group.GroupId, // Assign the new group ID
                CreateDate = DateTime.Now
            };
            await taskRepo.AddAsync(taskToAdd);

            // Delete sub task from the subtask list
            subTask.IsDeleted = true;
            await subTaskRepo.UpdateAsync(subTask);
            cacheModel.TaskId = taskToAdd.TaskId;
            cacheModel.SubTaskId = subTaskId;
            cacheModel.IsExistedGroup = false;
            cacheModel.MovedGroupId = group.GroupId;
            cacheService.SetData(ConstantVariables.CacheKeyForUndoSubTaskMoved, cacheModel, DateTimeOffset.Now.AddSeconds(3));
            return new()
            {
                IsSuccess = true,
                Message = "Subtask moved to new group successfully!!"
            };
        }

        public async Task<ResponseModel> ToggleStarSubTaskAsync(int subTaskId)
        {
            var subTask = await subTaskRepo.GetByIdAsync(subTaskId);
            if (subTask == null || subTask.IsDeleted)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Subtask not found"
                };
            }
            subTask.IsStarred = !subTask.IsStarred;
            await subTaskRepo.UpdateAsync(subTask);
            return new()
            {
                IsSuccess = true,
                Message = "Subtask starred status updated successfully",
                Data = subTask
            };
        }

        public async Task<ResponseModel> UpdateSubTaskAsync(int subTaskId, UpdateTaskRequestModel model)
        {
            var subTask = await subTaskRepo.GetByIdAsync(subTaskId);
            if (subTask == null || subTask.IsDeleted)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Subtask not found"
                };
            }
            subTask.Title = model.Title;
            subTask.Description = model.Description;
            subTask.ToDoDate = model.ToDoDate;
            if (subTask.IsCompleted)
                subTask.CompleteDate = DateTime.Now;
            else
                subTask.CompleteDate = null;
            await subTaskRepo.UpdateAsync(subTask);
            return new()
            {
                IsSuccess = true,
                Message = "Subtask updated successfully!!",
                Data = subTask
            };
        }

        public async Task<ResponseModel> UpdateSubTaskCompletionStatusAsync(int subTaskId)
        {
            var subTask = await subTaskRepo.GetByIdAsync(subTaskId);
            if (subTask == null || subTask.IsDeleted)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Subtask not found"
                };
            }

            var task = await taskRepo.GetByIdAsync(subTask.TaskId);
            if (task == null || task.IsDeleted)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Parent Task is not found"
                };
            }

            if (subTask.IsCompleted)
            {
                subTask.IsCompleted = false;
                subTask.CompleteDate = null;

                // If the subtask is being marked as incomplete, we also need to check if the parent task should be marked as incomplete.
                if (task.IsCompleted)
                {
                    task.IsCompleted = false;
                    task.CompleteDate = null;
                    await taskRepo.UpdateAsync(task);
                }
                await subTaskRepo.UpdateAsync(subTask);

            }
            else
            {
                subTask.IsCompleted = true;
                subTask.CompleteDate = DateTime.Now;
                await subTaskRepo.UpdateAsync(subTask);
            }

            return new()
            {
                IsSuccess = true,
                Message = "Subtask completion status updated successfully",
            };
        }
    }
}
