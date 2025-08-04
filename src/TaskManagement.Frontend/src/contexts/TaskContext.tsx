import React, { createContext, useContext, useReducer, useEffect } from 'react';
import { TaskResponse } from '../services/apiService';
import { signalRService } from '../services/signalRService';

interface TaskState {
  tasks: TaskResponse[];
  loading: boolean;
  error: string | null;
}

type TaskAction =
  | { type: 'SET_LOADING'; payload: boolean }
  | { type: 'SET_TASKS'; payload: TaskResponse[] }
  | { type: 'ADD_TASK'; payload: TaskResponse }
  | { type: 'UPDATE_TASK'; payload: TaskResponse }
  | { type: 'DELETE_TASK'; payload: string }
  | { type: 'SET_ERROR'; payload: string | null }
  | { type: 'NORMALIZE_ORDERS' };

const initialState: TaskState = {
  tasks: [],
  loading: false,
  error: null,
};

function taskReducer(state: TaskState, action: TaskAction): TaskState {
  switch (action.type) {
    case 'SET_LOADING':
      return { ...state, loading: action.payload };
    case 'SET_TASKS':
      return { ...state, tasks: action.payload, loading: false, error: null };
    case 'ADD_TASK':
      return { ...state, tasks: [...state.tasks, action.payload] };
    case 'UPDATE_TASK':
      const updatedTasks = state.tasks.map(task =>
        task.id === action.payload.id ? action.payload : task
      );
      
      // Sort tasks to maintain proper order within each status
      const sortedTasks = updatedTasks.sort((a, b) => {
        if (a.status !== b.status) {
          return a.status - b.status;
        }
        if (a.order !== b.order) {
          return a.order - b.order;
        }
        return new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
      });
      
      return {
        ...state,
        tasks: sortedTasks,
      };
    case 'DELETE_TASK':
      return {
        ...state,
        tasks: state.tasks.filter(task => task.id !== action.payload),
      };
    case 'NORMALIZE_ORDERS':
      // Group tasks by status and normalize their orders
      const tasksByStatus = state.tasks.reduce((acc, task) => {
        if (!acc[task.status]) {
          acc[task.status] = [];
        }
        acc[task.status].push(task);
        return acc;
      }, {} as Record<number, TaskResponse[]>);

      // Normalize orders within each status
      const normalizedTasks: TaskResponse[] = [];
      Object.keys(tasksByStatus).forEach(statusKey => {
        const status = parseInt(statusKey);
        const tasksInStatus = tasksByStatus[status]
          .sort((a, b) => {
            if (a.order !== b.order) {
              return a.order - b.order;
            }
            return new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
          })
          .map((task, index) => ({ ...task, order: index }));
        
        normalizedTasks.push(...tasksInStatus);
      });

      return {
        ...state,
        tasks: normalizedTasks,
      };
    case 'SET_ERROR':
      return { ...state, error: action.payload, loading: false };
    default:
      return state;
  }
}

interface TaskContextType {
  state: TaskState;
  dispatch: React.Dispatch<TaskAction>;
}

const TaskContext = createContext<TaskContextType | undefined>(undefined);

export function TaskProvider({ children }: { children: React.ReactNode }) {
  const [state, dispatch] = useReducer(taskReducer, initialState);

  useEffect(() => {
    // Set up SignalR event listeners
    const handleTaskCreated = (task: TaskResponse) => {
      dispatch({ type: 'ADD_TASK', payload: task });
    };

    const handleTaskUpdated = (task: TaskResponse) => {
      dispatch({ type: 'UPDATE_TASK', payload: task });
    };

    const handleTaskDeleted = (taskId: string) => {
      dispatch({ type: 'DELETE_TASK', payload: taskId });
    };

    signalRService.on('TaskCreated', handleTaskCreated);
    signalRService.on('TaskUpdated', handleTaskUpdated);
    signalRService.on('TaskDeleted', handleTaskDeleted);

    return () => {
      signalRService.off('TaskCreated', handleTaskCreated);
      signalRService.off('TaskUpdated', handleTaskUpdated);
      signalRService.off('TaskDeleted', handleTaskDeleted);
    };
  }, []);

  return (
    <TaskContext.Provider value={{ state, dispatch }}>
      {children}
    </TaskContext.Provider>
  );
}

export function useTask() {
  const context = useContext(TaskContext);
  if (!context) {
    throw new Error('useTask must be used within a TaskProvider');
  }
  return context;
}
