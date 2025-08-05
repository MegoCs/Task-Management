import React from 'react';
import { TaskResponse } from '../services/apiService';
import './TaskCard.css';

interface TaskCardProps {
  task: TaskResponse;
  onEdit: (task: TaskResponse) => void;
  onDelete: (taskId: string) => void;
}

const PRIORITY_COLORS = {
  0: '#28a745', // Low - Green
  1: '#ffc107', // Medium - Yellow
  2: '#fd7e14', // High - Orange
  3: '#dc3545', // Critical - Red
};

const PRIORITY_NAMES = {
  0: 'Low',
  1: 'Medium', 
  2: 'High',
  3: 'Critical',
};

function TaskCard({ task, onEdit, onDelete }: TaskCardProps) {
  const priorityColor = PRIORITY_COLORS[task.priority as keyof typeof PRIORITY_COLORS];
  const priorityName = PRIORITY_NAMES[task.priority as keyof typeof PRIORITY_NAMES];

  const formatDate = (dateString?: string) => {
    if (!dateString) return null;
    return new Date(dateString).toLocaleDateString();
  };

  const isOverdue = task.dueDate && new Date(task.dueDate) < new Date() && task.status !== 3;

  const handleCardClick = () => {
    onEdit(task);
  };

  return (
    <div 
      className={`task-card ${isOverdue ? 'overdue' : ''}`}
      onClick={handleCardClick}
      style={{ cursor: 'pointer' }}
    >
      <div className="task-header">
        <div className="task-priority" style={{ backgroundColor: priorityColor }}>
          {priorityName}
        </div>
        <div className="task-actions">
          <button
            className="btn-icon"
            onClick={(e) => {
              e.stopPropagation();
              onEdit(task);
            }}
            title="Edit"
          >
            ✏️
          </button>
          <button
            className="btn-icon"
            onClick={(e) => {
              e.stopPropagation();
              onDelete(task.id);
            }}
            title="Delete"
          >
            🗑️
          </button>
        </div>
      </div>

      <div className="task-content">
        <h4 className="task-title">{task.title}</h4>
        {task.description && (
          <p className="task-description">{task.description}</p>
        )}
      </div>

      <div className="task-footer">
        {task.assigneeEmail && (
          <div className="task-assignee">
            <span>👤 {task.assigneeEmail}</span>
          </div>
        )}
        {task.dueDate && (
          <div className={`task-due-date ${isOverdue ? 'overdue' : ''}`}>
            <span>📅 {formatDate(task.dueDate)}</span>
          </div>
        )}
      </div>

      {task.tags.length > 0 && (
        <div className="task-tags">
          {task.tags.map((tag, index) => (
            <span key={index} className="task-tag">
              {tag}
            </span>
          ))}
        </div>
      )}

      {task.comments.length > 0 && (
        <div className="task-comments-count">
          💬 {task.comments.length} comment{task.comments.length !== 1 ? 's' : ''}
        </div>
      )}
    </div>
  );
}

export default TaskCard;
