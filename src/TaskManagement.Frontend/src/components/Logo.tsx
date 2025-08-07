import React from 'react';
import './Logo.css';

interface LogoProps {
  className?: string;
  variant?: 'light' | 'dark' | 'colored';
  size?: 'small' | 'medium' | 'large' | 'xl';
}

const Logo: React.FC<LogoProps> = ({ 
  className = '',
  variant = 'colored',
  size = 'medium'
}) => {
  const logoClasses = [
    'logo',
    `logo-${variant}`,
    `logo-${size}`,
    className
  ].filter(Boolean).join(' ');

  return (
    <div className={logoClasses}>
      <img 
        src="/adamAi.png" 
        alt="adamAi Logo" 
        className="logo-image"
        aria-label="adamAi Task Manager Logo"
      />
    </div>
  );
};

export default Logo;
