import React, { useState, useEffect } from 'react';
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

  useEffect(() => {
    loadTasks();
  }, []);

  const loadTasks = async () => {
    try {
      dispatch({ type: 'SET_LOADING', payload: true });
      const tasks = await apiService.getTasks();
      dispatch({ type: 'SET_TASKS', payload: tasks });
    } catch (error) {
      console.error('Error loading tasks:', error);
      dispatch({ type: 'SET_ERROR', payload: 'Failed to load tasks' });
    }
  };

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
    const newOrder = destination.index;

    try {
      await apiService.updateTaskOrder(draggableId, {
        newOrder,
        newStatus: newStatus !== parseInt(source.droppableId) ? newStatus : undefined,
      });
    } catch (error) {
      console.error('Error updating task order:', error);
    }
  };

  const getTasksByStatus = (status: number) => {
    return state.tasks
      .filter(task => task.status === status)
      .sort((a, b) => a.order - b.order);
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
    return <div className="loading">Loading tasks...</div>;
  }

  if (state.error) {
    return <div className="error">Error: {state.error}</div>;
  }

  return (
    <div className="kanban-board">
      <div className="board-header">
        <h1>Task Board</h1>
        <button className="btn btn-primary" onClick={handleCreateTask}>
          + Add Task
        </button>
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
