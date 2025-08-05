import React, { useState, useEffect } from 'react';
import { TaskResponse, CreateTaskRequest, UpdateTaskRequest, TaskCommentResponse, apiService } from '../services/apiService';
import './TaskModal.css';

interface TaskModalProps {
  task?: TaskResponse;
  onClose: () => void;
  onSave: () => void;
}

interface CommentProps {
  comment: TaskCommentResponse;
  onReply: (commentId: string) => void;
  replyingTo: string | null;
  replyContent: string;
  onReplyContentChange: (content: string) => void;
  onSubmitReply: (parentCommentId: string) => void;
  onCancelReply: () => void;
  level?: number;
  collapsedReplies: Set<string>;
  onToggleCollapse: (commentId: string) => void;
}

// Helper function to generate avatar from user name/email
const generateAvatar = (name: string, email: string): string => {
  const initials = name.split(' ').map(n => n[0]?.toUpperCase()).join('').slice(0, 2) || 
                   email.split('@')[0].slice(0, 2).toUpperCase();
  
  // Generate a consistent color based on the email
  const hash = email.split('').reduce((a, b) => {
    a = ((a << 5) - a) + b.charCodeAt(0);
    return a & a;
  }, 0);
  
  const colors = [
    '#FF6B6B', '#4ECDC4', '#45B7D1', '#96CEB4', '#FFEAA7', 
    '#DDA0DD', '#98D8C8', '#F7DC6F', '#BB8FCE', '#85C1E9'
  ];
  
  const color = colors[Math.abs(hash) % colors.length];
  
  return `data:image/svg+xml;base64,${btoa(`
    <svg width="32" height="32" xmlns="http://www.w3.org/2000/svg">
      <circle cx="16" cy="16" r="16" fill="${color}"/>
      <text x="16" y="20" font-family="Arial, sans-serif" font-size="12" font-weight="bold" 
            text-anchor="middle" fill="white">${initials}</text>
    </svg>
  `)}`;
};

const Comment: React.FC<CommentProps> = ({
  comment,
  onReply,
  replyingTo,
  replyContent,
  onReplyContentChange,
  onSubmitReply,
  onCancelReply,
  level = 0,
  collapsedReplies,
  onToggleCollapse
}) => {
  const isReplying = replyingTo === comment.id;
  const maxLevel = 3; // Limit nesting to prevent excessive indentation
  const hasReplies = comment.replies && comment.replies.length > 0;
  const isCollapsed = collapsedReplies.has(comment.id);

  return (
    <div className={`comment ${level > 0 ? 'comment-reply' : ''}`} style={{ marginLeft: `${level * 20}px` }}>
      <div className="comment-header">
        <div className="comment-author">
          <img 
            src={generateAvatar(comment.authorName, comment.authorEmail)} 
            alt={comment.authorName}
            className="comment-avatar"
            title={`${comment.authorName} (${comment.authorEmail})`}
          />
          <strong>{comment.authorName}</strong>
        </div>
        <span className="comment-date">
          {new Date(comment.createdAt).toLocaleString()}
        </span>
      </div>
      <div className="comment-content">{comment.content}</div>
      
      <div className="comment-actions">
        {level < maxLevel && (
          <button 
            type="button"
            className="reply-btn"
            onClick={(e) => {
              e.preventDefault();
              e.stopPropagation();
              onReply(comment.id);
            }}
            disabled={isReplying}
          >
            Reply
          </button>
        )}
        
        {hasReplies && (
          <button 
            type="button"
            className="toggle-replies-btn"
            onClick={(e) => {
              e.preventDefault();
              e.stopPropagation();
              onToggleCollapse(comment.id);
            }}
          >
            {isCollapsed ? '▶' : '▼'} {comment.replies.length} {comment.replies.length === 1 ? 'reply' : 'replies'}
          </button>
        )}
      </div>

      {isReplying && (
        <div className="reply-form">
          <textarea
            value={replyContent}
            onChange={(e) => onReplyContentChange(e.target.value)}
            placeholder="Write a reply..."
            rows={2}
            className="reply-input"
          />
          <div className="reply-actions">
            <button
              type="button"
              className="btn btn-secondary btn-sm"
              onClick={(e) => {
                e.preventDefault();
                e.stopPropagation();
                onSubmitReply(comment.id);
              }}
              disabled={!replyContent.trim()}
            >
              Reply
            </button>
            <button
              type="button"
              className="btn btn-cancel btn-sm"
              onClick={(e) => {
                e.preventDefault();
                e.stopPropagation();
                onCancelReply();
              }}
            >
              Cancel
            </button>
          </div>
        </div>
      )}

      {hasReplies && !isCollapsed && (
        <div className="comment-replies">
          {comment.replies.map(reply => (
            <Comment
              key={reply.id}
              comment={reply}
              onReply={onReply}
              replyingTo={replyingTo}
              replyContent={replyContent}
              onReplyContentChange={onReplyContentChange}
              onSubmitReply={onSubmitReply}
              onCancelReply={onCancelReply}
              level={level + 1}
              collapsedReplies={collapsedReplies}
              onToggleCollapse={onToggleCollapse}
            />
          ))}
        </div>
      )}
    </div>
  );
};

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

  const [currentTask, setCurrentTask] = useState<TaskResponse | undefined>(task);
  const comments = currentTask?.comments || [];
  const [newComment, setNewComment] = useState('');
  const [replyingTo, setReplyingTo] = useState<string | null>(null);
  const [replyContent, setReplyContent] = useState('');
  const [collapsedReplies, setCollapsedReplies] = useState<Set<string>>(new Set());
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const isEditing = !!task;

  // Update internal state when task prop changes (for real-time updates)
  useEffect(() => {
    if (task && (!currentTask || task.id !== currentTask.id || JSON.stringify(task) !== JSON.stringify(currentTask))) {
      console.log('TaskModal: Task prop updated', { old: currentTask, new: task });
      setCurrentTask(task);
      // Also update form data if it's a different task
      if (!currentTask || task.id !== currentTask.id) {
        setFormData({
          title: task.title || '',
          description: task.description || '',
          assigneeEmail: task.assigneeEmail || '',
          dueDate: task.dueDate ? task.dueDate.split('T')[0] : '',
          priority: task.priority || 1,
          status: task.status || 0,
          tags: task.tags.join(', ') || '',
        });
      }
    }
  }, [task, currentTask]);

  // Initialize collapsed state for comments with replies (default to collapsed)
  useEffect(() => {
    if (currentTask?.comments) {
      const commentsWithReplies = new Set<string>();
      const collectCommentsWithReplies = (comments: TaskCommentResponse[]) => {
        comments.forEach(comment => {
          if (comment.replies && comment.replies.length > 0) {
            commentsWithReplies.add(comment.id);
            collectCommentsWithReplies(comment.replies);
          }
        });
      };
      collectCommentsWithReplies(currentTask.comments);
      setCollapsedReplies(commentsWithReplies);
    }
  }, [currentTask?.comments]);

  // Toggle collapse state for a comment's replies
  const handleToggleCollapse = (commentId: string) => {
    setCollapsedReplies(prev => {
      const newSet = new Set(prev);
      if (newSet.has(commentId)) {
        newSet.delete(commentId);
      } else {
        newSet.add(commentId);
      }
      return newSet;
    });
  };

  // Function to refresh task data without closing modal
  const refreshTaskData = async () => {
    if (!currentTask?.id) return;
    
    try {
      const updatedTask = await apiService.getTask(currentTask.id);
      setCurrentTask(updatedTask);
    } catch (error: any) {
      setError(error.response?.data?.message || 'Failed to refresh task data');
    }
  };

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
        await apiService.updateTask(currentTask!.id, updateRequest);
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
    if (!newComment.trim() || !currentTask) return;

    try {
      await apiService.addComment(currentTask.id, newComment);
      setNewComment('');
      // Refresh the task data to show the new comment without closing modal
      await refreshTaskData();
    } catch (error: any) {
      setError(error.response?.data?.message || 'Failed to add comment');
    }
  };

  const handleAddReply = async (parentCommentId: string) => {
    if (!replyContent.trim() || !currentTask) return;

    try {
      await apiService.addComment(currentTask.id, replyContent, parentCommentId);
      setReplyContent('');
      setReplyingTo(null);
      // Refresh the task data to show the new reply without closing modal
      await refreshTaskData();
    } catch (error: any) {
      setError(error.response?.data?.message || 'Failed to add reply');
    }
  };

  const handleStartReply = (commentId: string) => {
    setReplyingTo(commentId);
    setReplyContent('');
  };

  const handleCancelReply = () => {
    setReplyingTo(null);
    setReplyContent('');
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
                  <Comment
                    key={comment.id}
                    comment={comment}
                    onReply={handleStartReply}
                    replyingTo={replyingTo}
                    replyContent={replyContent}
                    onReplyContentChange={setReplyContent}
                    onSubmitReply={handleAddReply}
                    onCancelReply={handleCancelReply}
                    collapsedReplies={collapsedReplies}
                    onToggleCollapse={handleToggleCollapse}
                  />
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
                  onClick={(e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    handleAddComment();
                  }}
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
