import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';

export interface TaskUpdateEvent {
  type: 'TaskCreated' | 'TaskUpdated' | 'TaskDeleted';
  data: any;
}

class SignalRService {
  private connection: HubConnection | null = null;
  private listeners: { [key: string]: Function[] } = {};

  async start(token: string): Promise<void> {
    if (this.connection) {
      await this.stop();
    }

    const signalRUrl = process.env.REACT_APP_SIGNALR_URL || 'http://localhost:5000/taskHub';
    this.connection = new HubConnectionBuilder()
      .withUrl(signalRUrl, {
        accessTokenFactory: () => token
      })
      .build();

    // Set up event listeners
    this.connection.on('TaskCreated', (task) => {
      this.emit('TaskCreated', task);
    });

    this.connection.on('TaskUpdated', (task) => {
      this.emit('TaskUpdated', task);
    });

    this.connection.on('TaskDeleted', (taskId) => {
      this.emit('TaskDeleted', taskId);
    });

    this.connection.onclose(() => {
      console.log('SignalR connection closed');
    });

    try {
      await this.connection.start();
      console.log('SignalR connection started');
    } catch (error) {
      console.error('Error starting SignalR connection:', error);
    }
  }

  async stop(): Promise<void> {
    if (this.connection) {
      try {
        await this.connection.stop();
        console.log('SignalR connection stopped');
      } catch (error) {
        console.error('Error stopping SignalR connection:', error);
      }
      this.connection = null;
    }
  }

  on(event: string, callback: Function): void {
    if (!this.listeners[event]) {
      this.listeners[event] = [];
    }
    this.listeners[event].push(callback);
  }

  off(event: string, callback: Function): void {
    if (this.listeners[event]) {
      this.listeners[event] = this.listeners[event].filter(cb => cb !== callback);
    }
  }

  private emit(event: string, data: any): void {
    if (this.listeners[event]) {
      this.listeners[event].forEach(callback => callback(data));
    }
  }

  async joinUserGroup(userId: string): Promise<void> {
    if (this.connection) {
      try {
        await this.connection.invoke('JoinGroup', userId);
      } catch (error) {
        console.error('Error joining user group:', error);
      }
    }
  }
}

export const signalRService = new SignalRService();
