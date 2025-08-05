import React, { useState, useEffect, useCallback } from 'react';
import { DragDropContext, Droppable, Draggable } from 'react-beautiful-dnd';
import { TaskResponse, apiService } from '../services/apiService';
import { useTask } from '../contexts/TaskContext';
import TaskCard from './TaskCard';
import TaskModal from './TaskModal';
import './KanbanBoard.css';

const TASK_STATUSES = [
  { id: 0, name: 'Todo', title: 'To Do' },
  { id: 1, name: 'InProgress', title: 'In Progress' },
  { id: 2, name: 'Review', title: 'Review' },
  { id: 3, name: 'Done', title: 'Done' },
];

function KanbanBoard() {
  const { state, dispatch } = useTask();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingTask, setEditingTask] = useState<TaskResponse | undefined>();

  const loadTasks = useCallback(async () => {
    try {
      dispatch({ type: 'SET_LOADING', payload: true });
      const tasks = await apiService.getTasks();
      dispatch({ type: 'SET_TASKS', payload: tasks });
    } catch (error) {
      console.error('Error loading tasks:', error);
      dispatch({ type: 'SET_ERROR', payload: 'Failed to load tasks' });
    }
  }, [dispatch]);

  useEffect(() => {
    loadTasks();
  }, [loadTasks]);

  // Update editingTask when the task in context gets updated via SignalR
  useEffect(() => {
    if (editingTask && isModalOpen) {
      const updatedTask = state.tasks.find(task => task.id === editingTask.id);
      if (updatedTask && JSON.stringify(updatedTask) !== JSON.stringify(editingTask)) {
        console.log('KanbanBoard: Updating editingTask via SignalR', { old: editingTask, new: updatedTask });
        setEditingTask(updatedTask);
      }
    }
  }, [state.tasks, editingTask, isModalOpen]);

  const handleDragEnd = async (result: any) => {
    const { destination, source, draggableId } = result;

    if (!destination) return;

    if (
      destination.droppableId === source.droppableId &&
      destination.index === source.index
    ) {
      return;
    }

    const newStatus = parseInt(destination.droppableId);
    const oldStatus = parseInt(source.droppableId);
    const newIndex = destination.index;

    console.log('Drag operation:', {
      taskId: draggableId,
      oldStatus,
      newStatus,
      oldIndex: source.index,
      newIndex: newIndex
    });

    // Find the task being moved
    const taskToMove = state.tasks.find(task => task.id === draggableId);
    if (!taskToMove) {
      console.error('Task not found:', draggableId);
      return;
    }

    // Create a copy of all tasks for manipulation
    let updatedTasks = [...state.tasks];
    
    // If moving within the same column
    if (oldStatus === newStatus) {
      // Get all tasks in the same column, sorted by current order
      const tasksInColumn = updatedTasks
        .filter(task => task.status === newStatus)
        .sort((a, b) => a.order - b.order);

      console.log('Before reorder:', tasksInColumn.map(t => ({ id: t.id, title: t.title, order: t.order })));

      // Find the task to move
      const taskIndex = tasksInColumn.findIndex(task => task.id === draggableId);
      if (taskIndex >= 0) {
        // Remove the task from its current position
        const [movedTask] = tasksInColumn.splice(taskIndex, 1);
        
        // Insert the task at the new position
        tasksInColumn.splice(newIndex, 0, movedTask);
        
        // Reassign order values based on new positions
        tasksInColumn.forEach((task, index) => {
          task.order = index;
        });

        console.log('After reorder:', tasksInColumn.map(t => ({ id: t.id, title: t.title, order: t.order })));

        // Update the main tasks array with the reordered tasks
        updatedTasks = updatedTasks.map(task => {
          if (task.status === newStatus) {
            const reorderedTask = tasksInColumn.find(t => t.id === task.id);
            return reorderedTask || task;
          }
          return task;
        });
      }
    } else {
      // Moving between different columns
      
      // Get tasks in destination column to determine correct insertion point
      const destTasks = updatedTasks
        .filter(task => task.status === newStatus && task.id !== draggableId)
        .sort((a, b) => a.order - b.order);

      // Insert the moved task at the correct position
      destTasks.splice(newIndex, 0, { ...taskToMove, status: newStatus, order: newIndex });

      // Reassign order values for destination column
      destTasks.forEach((task, index) => {
        task.order = index;
      });

      // Reorder tasks in the source column (close gaps)
      const sourceTasks = updatedTasks
        .filter(task => task.status === oldStatus && task.id !== draggableId)
        .sort((a, b) => a.order - b.order);

      sourceTasks.forEach((task, index) => {
        task.order = index;
      });

      // Update the main tasks array
      updatedTasks = updatedTasks.map(task => {
        if (task.id === draggableId) {
          // Find the moved task in destTasks
          const movedTask = destTasks.find(t => t.id === task.id);
          return movedTask || task;
        }
        if (task.status === newStatus) {
          const updatedTask = destTasks.find(t => t.id === task.id);
          return updatedTask || task;
        }
        if (task.status === oldStatus) {
          const updatedTask = sourceTasks.find(t => t.id === task.id);
          return updatedTask || task;
        }
        return task;
      });
    }

    // Update the state optimistically
    dispatch({ type: 'SET_TASKS', payload: updatedTasks });

    try {
      // Use the new order from the reordered task
      const reorderedTask = updatedTasks.find(t => t.id === draggableId);
      const finalOrder = reorderedTask ? reorderedTask.order : newIndex;
      
      await apiService.updateTaskOrder(draggableId, {
        newOrder: finalOrder,
        newStatus: newStatus !== oldStatus ? newStatus : undefined,
      });
      
      console.log('Task order updated successfully');
      
      // Optionally reload tasks to ensure backend consistency
      // Commented out to improve UX, but can be enabled for debugging
      // setTimeout(() => loadTasks(), 500);
      
    } catch (error) {
      console.error('Error updating task order:', error);
      // Revert the optimistic update by reloading tasks
      await loadTasks();
    }
  };

  const getTasksByStatus = (status: number) => {
    const tasksInStatus = state.tasks
      .filter(task => task.status === status)
      .sort((a, b) => {
        // Primary sort by order
        if (a.order !== b.order) {
          return a.order - b.order;
        }
        // Secondary sort by creation date for stability
        return new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
      });

    // Debug logging (can be removed in production)
    if (tasksInStatus.length > 1) {
      console.log(`Tasks in status ${status}:`, tasksInStatus.map(t => ({ id: t.id, title: t.title, order: t.order })));
    }

    // Ensure orders are sequential (fix any gaps) - but don't modify the original objects
    return tasksInStatus.map((task, index) => ({
      ...task,
      // Don't override the order here, let the drag handler manage it
      // order: index
    }));
  };

  const debugTaskOrders = () => {
    console.log('=== Task Order Debug ===');
    TASK_STATUSES.forEach(status => {
      const tasks = getTasksByStatus(status.id);
      console.log(`${status.title} (${status.id}):`, tasks.map(t => `${t.title}(${t.order})`).join(', '));
    });
    console.log('======================');
  };

  const getTaskStatistics = () => {
    const total = state.tasks.length;
    const todo = state.tasks.filter(task => task.status === 0).length;
    const inProgress = state.tasks.filter(task => task.status === 1).length;
    const review = state.tasks.filter(task => task.status === 2).length;
    const done = state.tasks.filter(task => task.status === 3).length;
    const highPriority = state.tasks.filter(task => task.priority >= 2).length;
    const overdue = state.tasks.filter(task => {
      if (!task.dueDate) return false;
      return new Date(task.dueDate) < new Date() && task.status !== 3;
    }).length;

    return { total, todo, inProgress, review, done, highPriority, overdue };
  };

  const handleCreateTask = () => {
    setEditingTask(undefined);
    setIsModalOpen(true);
  };

  const handleEditTask = (task: TaskResponse) => {
    setEditingTask(task);
    setIsModalOpen(true);
  };

  const handleDeleteTask = async (taskId: string) => {
    if (window.confirm('Are you sure you want to delete this task?')) {
      try {
        await apiService.deleteTask(taskId);
      } catch (error) {
        console.error('Error deleting task:', error);
      }
    }
  };

  if (state.loading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner">Loading your tasks...</div>
      </div>
    );
  }

  if (state.error) {
    return (
      <div className="error-container">
        <div className="error-message">
          <h3>Oops! Something went wrong</h3>
          <p>{state.error}</p>
          <button className="btn btn-primary" onClick={loadTasks}>
            Try Again
          </button>
        </div>
      </div>
    );
  }

  const stats = getTaskStatistics();

  return (
    <div className="kanban-board">
      <div className="board-header">
        <div className="header-left">
          <h1>Task Board</h1>
          <div className="board-stats">
            <div className="stat-item">
              <span className="stat-number">{stats.total}</span>
              <span className="stat-label">Total Tasks</span>
            </div>
            <div className="stat-item">
              <span className="stat-number">{stats.done}</span>
              <span className="stat-label">Completed</span>
            </div>
            {stats.overdue > 0 && (
              <div className="stat-item warning">
                <span className="stat-number">{stats.overdue}</span>
                <span className="stat-label">Overdue</span>
              </div>
            )}
            {stats.highPriority > 0 && (
              <div className="stat-item priority">
                <span className="stat-number">{stats.highPriority}</span>
                <span className="stat-label">High Priority</span>
              </div>
            )}
          </div>
        </div>
        <div className="header-actions">
          {process.env.NODE_ENV === 'development' && (
            <button className="btn btn-secondary" onClick={debugTaskOrders} style={{ marginRight: '10px' }}>
              🐛 Debug Orders
            </button>
          )}
          <button className="btn add-task-btn" onClick={handleCreateTask}>
            ✨ Add Task
          </button>
        </div>
      </div>

      <DragDropContext onDragEnd={handleDragEnd}>
        <div className="board-columns">
          {TASK_STATUSES.map(status => (
            <div key={status.id} className="column">
              <div className="column-header">
                <h3>{status.title}</h3>
                <span className="task-count">
                  {getTasksByStatus(status.id).length}
                </span>
              </div>

              <Droppable droppableId={status.id.toString()}>
                {(provided, snapshot) => (
                  <div
                    ref={provided.innerRef}
                    {...provided.droppableProps}
                    className={`column-content ${
                      snapshot.isDraggingOver ? 'dragging-over' : ''
                    }`}
                  >
                    {getTasksByStatus(status.id).length === 0 && stats.total === 0 && status.id === 0 ? (
                      <div className="empty-column-message">
                        <p>🚀 Welcome to your task board!</p>
                        <p>Click "Add Task" above to get started.</p>
                      </div>
                    ) : getTasksByStatus(status.id).length === 0 ? (
                      <div className="empty-column">
                        <p>No tasks in {status.title.toLowerCase()}</p>
                      </div>
                    ) : null}
                    
                    {getTasksByStatus(status.id).map((task, index) => (
                      <Draggable
                        key={task.id}
                        draggableId={task.id}
                        index={index}
                      >
                        {(provided, snapshot) => (
                          <div
                            ref={provided.innerRef}
                            {...provided.draggableProps}
                            {...provided.dragHandleProps}
                            className={`task-item ${
                              snapshot.isDragging ? 'dragging' : ''
                            }`}
                          >
                            <TaskCard
                              task={task}
                              onEdit={handleEditTask}
                              onDelete={handleDeleteTask}
                            />
                          </div>
                        )}
                      </Draggable>
                    ))}
                    {provided.placeholder}
                  </div>
                )}
              </Droppable>
            </div>
          ))}
        </div>
      </DragDropContext>

      {isModalOpen && (
        <TaskModal
          task={editingTask}
          onClose={() => setIsModalOpen(false)}
          onSave={() => {
            setIsModalOpen(false);
            loadTasks();
          }}
        />
      )}
    </div>
  );
}

export default KanbanBoard;
