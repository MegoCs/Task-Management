import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';

export interface TaskResponse {
  id: string;
  title: string;
  description: string;
  status: number;
  statusName: string;
  priority: number;
  priorityName: string;
  assigneeId?: string;
  assigneeEmail?: string;
  createdById: string;
  dueDate?: string;
  order: number;
  createdAt: string;
  updatedAt: string;
  completedAt?: string;
  tags: string[];
  comments: TaskCommentResponse[];
}

export interface TaskCommentResponse {
  id: string;
  authorId: string;
  authorName: string;
  content: string;
  createdAt: string;
}

export interface CreateTaskRequest {
  title: string;
  description: string;
  assigneeEmail?: string;
  dueDate?: string;
  priority: number;
  tags: string[];
}

export interface UpdateTaskRequest {
  title?: string;
  description?: string;
  assigneeEmail?: string;
  dueDate?: string;
  priority?: number;
  status?: number;
  tags?: string[];
}

export interface UpdateTaskOrderRequest {
  newOrder: number;
  newStatus?: number;
}

export interface AuthResponse {
  token: string;
  userId: string;
  name: string;
  email: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
}

class ApiService {
  private baseURL: string;
  private token: string | null = null;

  constructor() {
    this.baseURL = API_BASE_URL;
    this.token = localStorage.getItem('token');
  }

  setToken(token: string) {
    this.token = token;
    localStorage.setItem('token', token);
  }

  clearToken() {
    this.token = null;
    localStorage.removeItem('token');
  }

  private getHeaders() {
    const headers: any = {
      'Content-Type': 'application/json',
    };
    
    if (this.token) {
      headers.Authorization = `Bearer ${this.token}`;
    }
    
    return headers;
  }

  // Auth methods
  async login(request: LoginRequest): Promise<AuthResponse> {
    const response = await axios.post(`${this.baseURL}/auth/login`, request);
    return response.data;
  }

  async register(request: RegisterRequest): Promise<AuthResponse> {
    const response = await axios.post(`${this.baseURL}/auth/register`, request);
    return response.data;
  }

  // Task methods
  async getTasks(): Promise<TaskResponse[]> {
    const response = await axios.get(`${this.baseURL}/tasks`, {
      headers: this.getHeaders(),
    });
    return response.data;
  }

  async getTask(id: string): Promise<TaskResponse> {
    const response = await axios.get(`${this.baseURL}/tasks/${id}`, {
      headers: this.getHeaders(),
    });
    return response.data;
  }

  async createTask(request: CreateTaskRequest): Promise<TaskResponse> {
    const response = await axios.post(`${this.baseURL}/tasks`, request, {
      headers: this.getHeaders(),
    });
    return response.data;
  }

  async updateTask(id: string, request: UpdateTaskRequest): Promise<TaskResponse> {
    const response = await axios.put(`${this.baseURL}/tasks/${id}`, request, {
      headers: this.getHeaders(),
    });
    return response.data;
  }

  async deleteTask(id: string): Promise<void> {
    await axios.delete(`${this.baseURL}/tasks/${id}`, {
      headers: this.getHeaders(),
    });
  }

  async updateTaskOrder(id: string, request: UpdateTaskOrderRequest): Promise<void> {
    await axios.put(`${this.baseURL}/tasks/${id}/order`, request, {
      headers: this.getHeaders(),
    });
  }

  async addComment(id: string, content: string): Promise<TaskResponse> {
    const response = await axios.post(`${this.baseURL}/tasks/${id}/comments`, 
      { content }, 
      { headers: this.getHeaders() }
    );
    return response.data;
  }
}

export const apiService = new ApiService();
