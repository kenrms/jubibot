import React, { useState } from 'react';
import styled from 'styled-components';
import { FaChevronDown, FaChevronRight } from 'react-icons/fa';

const Container = styled.div`
  border: 1px solid #ccc;
  margin-bottom: 10px;
  width: ${props => props.width || '100%'};
`;

const Header = styled.div`
  display: flex;
  align-items: center; /* Align items vertically */
  justify-content: space-between;
  padding: 10px;
  background-color: #f0f0f0;
  cursor: pointer;
  color: black;
  font-weight: bold;
`;

const Content = styled.div`
  padding: 10px;
  display: ${props => (props.collapsed ? 'none' : 'block')};
`;

const CollapsibleContainer = ({ title, children, width }) => {
    const [collapsed, setCollapsed] = useState(false);

    const toggleCollapsed = () => {
        setCollapsed(!collapsed);
    };

    return (
        <Container width={width}>
            <Header onClick={toggleCollapsed}>
                <div>{title}</div>
                {collapsed ? <FaChevronRight /> : <FaChevronDown />}
            </Header>
            <Content collapsed={collapsed}>{children}</Content>
        </Container>
    );
};

export default CollapsibleContainer;
