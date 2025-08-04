// MongoDB initialization script
db = db.getSiblingDB('TaskManagementDB');

// Create user for the application
db.createUser({
  user: 'taskuser',
  pwd: 'taskpass',
  roles: [
    {
      role: 'readWrite',
      db: 'TaskManagementDB'
    }
  ]
});

// Create collections with validation
db.createCollection('Tasks', {
  validator: {
    $jsonSchema: {
      bsonType: 'object',
      required: ['Title', 'Status', 'Priority', 'CreatedAt', 'UserId'],
      properties: {
        Title: { bsonType: 'string' },
        Description: { bsonType: 'string' },
        Status: { enum: ['Todo', 'InProgress', 'Done'] },
        Priority: { enum: ['Low', 'Medium', 'High', 'Critical'] },
        DueDate: { bsonType: 'date' },
        CreatedAt: { bsonType: 'date' },
        UpdatedAt: { bsonType: 'date' },
        UserId: { bsonType: 'string' },
        AssignedTo: { bsonType: 'string' },
        Comments: { bsonType: 'array' }
      }
    }
  }
});

db.createCollection('Users', {
  validator: {
    $jsonSchema: {
      bsonType: 'object',
      required: ['Username', 'Email', 'PasswordHash', 'CreatedAt'],
      properties: {
        Username: { bsonType: 'string' },
        Email: { bsonType: 'string' },
        PasswordHash: { bsonType: 'string' },
        FirstName: { bsonType: 'string' },
        LastName: { bsonType: 'string' },
        CreatedAt: { bsonType: 'date' },
        UpdatedAt: { bsonType: 'date' }
      }
    }
  }
});

// Create indexes for better performance
db.Tasks.createIndex({ UserId: 1 });
db.Tasks.createIndex({ Status: 1 });
db.Tasks.createIndex({ DueDate: 1 });
db.Tasks.createIndex({ CreatedAt: -1 });

db.Users.createIndex({ Username: 1 }, { unique: true });
db.Users.createIndex({ Email: 1 }, { unique: true });

print('Database initialization completed successfully!');
