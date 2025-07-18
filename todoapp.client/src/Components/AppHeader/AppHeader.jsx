import React, { useEffect, useRef,useContext } from 'react'
import { Context } from '../../global/MyContext';

import { CContainer, CDropdown, CDropdownItem, CDropdownMenu, CDropdownToggle, CHeader, CHeaderNav, CHeaderToggler, CNavItem, useColorModes, CFormInput } from '@coreui/react'
import CIcon from '@coreui/icons-react'
import {cilContrast,cilMenu,cilMoon,cilSun,} from '@coreui/icons'

const AppHeader = () => {
    const { sidebarShow, setSidebarShow, setSearchTerm } = useContext(Context);
    const headerRef = useRef()
    const { colorMode, setColorMode } = useColorModes('coreui-free-react-admin-template-theme')

    useEffect(() => {
        document.addEventListener('scroll', () => {
            headerRef.current &&
                headerRef.current.classList.toggle('shadow-sm', document.documentElement.scrollTop > 0)
        })
    }, [])

    return (
        <CHeader position="sticky" className="mb-4 p-0" ref={headerRef}>
            <CContainer className="border-bottom px-4" fluid>
                <CHeaderToggler
                    onClick={() => setSidebarShow(!sidebarShow)}
                    style={{ marginInlineStart: '-14px' }}
                >
                    <CIcon icon={cilMenu} size="lg" />
                </CHeaderToggler>
                <CHeaderNav className="d-none d-md-flex text-left">
                    <CNavItem className="ms-5">
                        <CFormInput
                            onChange={(e) => setSearchTerm(e.target.value) }
                            type="search"
                            id="searchInput"
                            placeholder="search..."
                            aria-describedby="exampleFormControlInputHelpInline"
                        />
                    </CNavItem>
                </CHeaderNav>
                <CHeaderNav className="ms-auto">
                </CHeaderNav>
                <CHeaderNav>
                    <li className="nav-item py-1">
                        <div className="vr h-100 mx-2 text-body text-opacity-75"></div>
                    </li>
                    <CDropdown variant="nav-item" placement="bottom-end">
                        <CDropdownToggle caret={false}>
                            {colorMode === 'dark' ? (
                                <CIcon icon={cilMoon} size="lg" />
                            ) : colorMode === 'auto' ? (
                                <CIcon icon={cilContrast} size="lg" />
                            ) : (
                                <CIcon icon={cilSun} size="lg" />
                            )}
                        </CDropdownToggle>
                        <CDropdownMenu>
                            <CDropdownItem
                                active={colorMode === 'light'}
                                className="d-flex align-items-center"
                                as="button"
                                type="button"
                                onClick={() => setColorMode('light')}
                            >
                                <CIcon className="me-2" icon={cilSun} size="lg" /> Light
                            </CDropdownItem>
                            <CDropdownItem
                                active={colorMode === 'dark'}
                                className="d-flex align-items-center"
                                as="button"
                                type="button"
                                onClick={() => setColorMode('dark')}
                            >
                                <CIcon className="me-2" icon={cilMoon} size="lg" /> Dark
                            </CDropdownItem>
                            <CDropdownItem
                                active={colorMode === 'auto'}
                                className="d-flex align-items-center"
                                as="button"
                                type="button"
                                onClick={() => setColorMode('auto')}
                            >
                                <CIcon className="me-2" icon={cilContrast} size="lg" /> Auto
                            </CDropdownItem>
                        </CDropdownMenu>
                    </CDropdown>
                    <li className="nav-item py-1">
                        <div className="vr h-100 mx-2 text-body text-opacity-75"></div>
                    </li>
                </CHeaderNav>
            </CContainer>
        </CHeader>
    )
}

export default AppHeader
