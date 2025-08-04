import React, { useState, useEffect } from 'react';
import { TaskResponse, CreateTaskRequest, UpdateTaskRequest, apiService } from '../services/apiService';
import './TaskModal.css';

interface TaskModalProps {
  task?: TaskResponse;
  onClose: () => void;
  onSave: () => void;
}

const PRIORITIES = [
  { value: 0, label: 'Low' },
  { value: 1, label: 'Medium' },
  { value: 2, label: 'High' },
  { value: 3, label: 'Critical' },
];

const STATUSES = [
  { value: 0, label: 'Todo' },
  { value: 1, label: 'In Progress' },
  { value: 2, label: 'Review' },
  { value: 3, label: 'Done' },
];

function TaskModal({ task, onClose, onSave }: TaskModalProps) {
  const [formData, setFormData] = useState({
    title: task?.title || '',
    description: task?.description || '',
    assigneeEmail: task?.assigneeEmail || '',
    dueDate: task?.dueDate ? task.dueDate.split('T')[0] : '',
    priority: task?.priority || 1,
    status: task?.status || 0,
    tags: task?.tags.join(', ') || '',
  });

  const [comments, setComments] = useState(task?.comments || []);
  const [newComment, setNewComment] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const isEditing = !!task;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const tagsArray = formData.tags
        .split(',')
        .map(tag => tag.trim())
        .filter(tag => tag.length > 0);

      if (isEditing) {
        const updateRequest: UpdateTaskRequest = {
          title: formData.title,
          description: formData.description,
          assigneeEmail: formData.assigneeEmail || undefined,
          dueDate: formData.dueDate || undefined,
          priority: formData.priority,
          status: formData.status,
          tags: tagsArray,
        };
        await apiService.updateTask(task.id, updateRequest);
      } else {
        const createRequest: CreateTaskRequest = {
          title: formData.title,
          description: formData.description,
          assigneeEmail: formData.assigneeEmail || undefined,
          dueDate: formData.dueDate || undefined,
          priority: formData.priority,
          tags: tagsArray,
        };
        await apiService.createTask(createRequest);
      }

      onSave();
    } catch (error: any) {
      setError(error.response?.data?.message || 'Failed to save task');
    } finally {
      setLoading(false);
    }
  };

  const handleAddComment = async () => {
    if (!newComment.trim() || !task) return;

    try {
      await apiService.addComment(task.id, newComment);
      // The comment will be updated via SignalR
      setNewComment('');
    } catch (error: any) {
      setError(error.response?.data?.message || 'Failed to add comment');
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name === 'priority' || name === 'status' ? parseInt(value) : value,
    }));
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{isEditing ? 'Edit Task' : 'Create New Task'}</h2>
          <button className="modal-close" onClick={onClose}>
            ×
          </button>
        </div>

        {error && <div className="error-message">{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="title">Title *</label>
            <input
              type="text"
              id="title"
              name="title"
              value={formData.title}
              onChange={handleChange}
              required
              placeholder="Enter task title"
            />
          </div>

          <div className="form-group">
            <label htmlFor="description">Description</label>
            <textarea
              id="description"
              name="description"
              value={formData.description}
              onChange={handleChange}
              rows={3}
              placeholder="Enter task description"
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="assigneeEmail">Assignee Email</label>
              <input
                type="email"
                id="assigneeEmail"
                name="assigneeEmail"
                value={formData.assigneeEmail}
                onChange={handleChange}
                placeholder="user@example.com"
              />
            </div>

            <div className="form-group">
              <label htmlFor="dueDate">Due Date</label>
              <input
                type="date"
                id="dueDate"
                name="dueDate"
                value={formData.dueDate}
                onChange={handleChange}
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="priority">Priority</label>
              <select
                id="priority"
                name="priority"
                value={formData.priority}
                onChange={handleChange}
              >
                {PRIORITIES.map(priority => (
                  <option key={priority.value} value={priority.value}>
                    {priority.label}
                  </option>
                ))}
              </select>
            </div>

            {isEditing && (
              <div className="form-group">
                <label htmlFor="status">Status</label>
                <select
                  id="status"
                  name="status"
                  value={formData.status}
                  onChange={handleChange}
                >
                  {STATUSES.map(status => (
                    <option key={status.value} value={status.value}>
                      {status.label}
                    </option>
                  ))}
                </select>
              </div>
            )}
          </div>

          <div className="form-group">
            <label htmlFor="tags">Tags (comma-separated)</label>
            <input
              type="text"
              id="tags"
              name="tags"
              value={formData.tags}
              onChange={handleChange}
              placeholder="frontend, urgent, bug"
            />
          </div>

          {isEditing && comments.length > 0 && (
            <div className="comments-section">
              <h3>Comments</h3>
              <div className="comments-list">
                {comments.map(comment => (
                  <div key={comment.id} className="comment">
                    <div className="comment-header">
                      <strong>{comment.authorName}</strong>
                      <span className="comment-date">
                        {new Date(comment.createdAt).toLocaleString()}
                      </span>
                    </div>
                    <div className="comment-content">{comment.content}</div>
                  </div>
                ))}
              </div>
            </div>
          )}

          {isEditing && (
            <div className="add-comment-section">
              <h3>Add Comment</h3>
              <div className="comment-input-group">
                <textarea
                  value={newComment}
                  onChange={(e) => setNewComment(e.target.value)}
                  placeholder="Add a comment..."
                  rows={2}
                />
                <button
                  type="button"
                  onClick={handleAddComment}
                  disabled={!newComment.trim()}
                  className="btn btn-secondary"
                >
                  Add Comment
                </button>
              </div>
            </div>
          )}

          <div className="modal-actions">
            <button type="button" className="btn btn-secondary" onClick={onClose}>
              Cancel
            </button>
            <button type="submit" className="btn btn-primary" disabled={loading}>
              {loading ? 'Saving...' : isEditing ? 'Update Task' : 'Create Task'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default TaskModal;
