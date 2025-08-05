# Task Management API Complete Seeding Script (PowerShell)
# ===========================================================
# This comprehensive script seeds the entire Task Management system with:
# 
# 👥 USERS (5 total):
#    - Admin User (admin@taskmanagement.com) - System administrator
#    - Project Manager (pm@taskmanagement.com) - Project oversight
#    - John Developer (john.dev@taskmanagement.com) - Backend development
#    - Sarah Designer (sarah.design@taskmanagement.com) - UI/UX design
#    - Mike QA (mike.qa@taskmanagement.com) - Quality assurance
#
# 📋 TASKS (6 total):
#    - Security Audit (Critical priority)
#    - Dashboard Redesign (Medium priority)
#    - API Versioning (High priority)
#    - Database Optimization (High priority)
#    - Automated Testing (High priority)
#    - Memory Leak Bug Fix (Critical priority)
#
# 💬 THREADED COMMENTS:
#    - Realistic workplace conversations between different user roles
#    - Multi-level threading (parent → child → grandchild)
#    - Natural discussion patterns for each task type
#
# 🚀 USAGE:
#    1. Ensure Docker containers are running: docker-compose up -d
#    2. Run this script: .\test-api.ps1
#    3. Test the system at http://localhost:3000
#
# ✨ FEATURES:
#    - Handles existing users (login if exists, register if new)
#    - Clears existing tasks for clean re-seeding
#    - Error handling with detailed feedback
#    - Progress indicators and success confirmations
#    - Complete verification with summary report
# ===========================================================

# Make sure your Docker containers are running: docker-compose up -d

Write-Host "=== Task Management API Complete Seeding Script ===" -ForegroundColor Blue
Write-Host "This script will create users, tasks, and realistic threaded comments" -ForegroundColor Yellow
Write-Host

# API URLs
$AUTH_API = "http://localhost:5001/api/auth"
$TASKS_API = "http://localhost:5000/api/tasks"

# Function to handle API errors gracefully
function Invoke-SafeRestMethod {
    param(
        [string]$Uri,
        [string]$Method,
        [hashtable]$Headers = @{},
        [string]$Body = "",
        [string]$ContentType = "application/json"
    )
    
    try {
        if ($Body) {
            return Invoke-RestMethod -Uri $Uri -Method $Method -Headers $Headers -Body $Body -ContentType $ContentType
        } else {
            return Invoke-RestMethod -Uri $Uri -Method $Method -Headers $Headers
        }
    }
    catch {
        Write-Host "API Error: $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

# Function to get or create user (try login first, then register if needed)
function Get-OrCreateUser {
    param(
        [string]$Name,
        [string]$Email,
        [string]$Password
    )
    
    # Try to login first
    $loginResponse = Invoke-SafeRestMethod -Uri "$AUTH_API/login" -Method "Post" -Body @"
{
  "email": "$Email",
  "password": "$Password"
}
"@
    
    if ($loginResponse -and $loginResponse.token) {
        Write-Host "   ✅ Logged in existing user: $Name" -ForegroundColor Green
        return $loginResponse.token
    }
    
    # If login failed, try to register
    $registerResponse = Invoke-SafeRestMethod -Uri "$AUTH_API/register" -Method "Post" -Body @"
{
  "name": "$Name",
  "email": "$Email",
  "password": "$Password"
}
"@
    
    if ($registerResponse -and $registerResponse.token) {
        Write-Host "   ✅ Registered new user: $Name" -ForegroundColor Green
        return $registerResponse.token
    }
    
    Write-Host "   ❌ Failed to login or register user: $Name" -ForegroundColor Red
    return $null
}

Write-Host "Creating test users..." -ForegroundColor Blue
Write-Host

# 1. Get or Create Users
Write-Host "1. Getting Admin User" -ForegroundColor Green
$adminToken = Get-OrCreateUser -Name "Admin User" -Email "admin@taskmanagement.com" -Password "Admin123!"
Write-Host

Write-Host "2. Getting Project Manager" -ForegroundColor Green
$pmToken = Get-OrCreateUser -Name "Project Manager" -Email "pm@taskmanagement.com" -Password "Manager123!"
Write-Host

Write-Host "3. Getting Developer" -ForegroundColor Green
$devToken = Get-OrCreateUser -Name "John Developer" -Email "john.dev@taskmanagement.com" -Password "Dev123!"
Write-Host

Write-Host "4. Getting Designer" -ForegroundColor Green
$designToken = Get-OrCreateUser -Name "Sarah Designer" -Email "sarah.design@taskmanagement.com" -Password "Design123!"
Write-Host

Write-Host "5. Getting QA Tester" -ForegroundColor Green
$qaToken = Get-OrCreateUser -Name "Mike QA" -Email "mike.qa@taskmanagement.com" -Password "QA123!"
Write-Host

# Store user tokens for easy access
$script:UserTokens = @{
    "Admin" = $adminToken
    "PM" = $pmToken
    "Developer" = $devToken
    "Designer" = $designToken
    "QA" = $qaToken
}

# Validate that we have all tokens
$allTokensValid = $true
foreach ($tokenPair in $UserTokens.GetEnumerator()) {
    if (-not $tokenPair.Value) {
        Write-Host "❌ Missing token for $($tokenPair.Key)" -ForegroundColor Red
        $allTokensValid = $false
    }
}

if (-not $allTokensValid) {
    Write-Host "❌ Cannot proceed without all user tokens. Please check your Docker services and try again." -ForegroundColor Red
    exit 1
}

Write-Host "✅ All user tokens obtained successfully!" -ForegroundColor Green

# Wait a moment for tokens to be properly issued
Start-Sleep -Seconds 2

# Function to clear existing tasks (optional)
function Clear-ExistingTasks {
    param([string]$Token)
    
    Write-Host "Checking for existing tasks to clear..." -ForegroundColor Yellow
    $headers = @{ "Authorization" = "Bearer $Token" }
    $existingTasks = Invoke-SafeRestMethod -Uri $TASKS_API -Method "Get" -Headers $headers
    
    if ($existingTasks -and $existingTasks.Count -gt 0) {
        Write-Host "Found $($existingTasks.Count) existing tasks. Clearing them..." -ForegroundColor Yellow
        foreach ($task in $existingTasks) {
            $deleteResponse = Invoke-SafeRestMethod -Uri "$TASKS_API/$($task.id)" -Method "Delete" -Headers $headers
            if ($deleteResponse) {
                Write-Host "   ✅ Deleted task: $($task.title)" -ForegroundColor Gray
            }
        }
        Write-Host "✅ All existing tasks cleared" -ForegroundColor Green
    } else {
        Write-Host "✅ No existing tasks found" -ForegroundColor Green
    }
    Write-Host
}

# Clear existing tasks before creating new ones
Clear-ExistingTasks -Token $UserTokens.Admin

Write-Host "Creating test tasks..." -ForegroundColor Blue
Write-Host

# 2. Create Tasks
Write-Host "6. Creating High Priority Task - Security Audit" -ForegroundColor Green
$headers = @{ "Authorization" = "Bearer $($UserTokens.Admin)" }
$task1 = Invoke-SafeRestMethod -Uri $TASKS_API -Method "Post" -Headers $headers -Body @'
{
  "title": "Security Audit - Critical Vulnerabilities",
  "description": "Conduct comprehensive security audit of the application to identify and fix critical vulnerabilities. This includes penetration testing, code review, and dependency scanning.",
  "assigneeEmail": "john.dev@taskmanagement.com",
  "dueDate": "2025-08-15T09:00:00Z",
  "priority": 3,
  "tags": ["security", "audit", "critical", "backend"]
}
'@

if ($task1) {
    Write-Host "✅ Security Audit task created - ID: $($task1.id)" -ForegroundColor Green
    $task1Id = $task1.id
} else {
    Write-Host "❌ Failed to create Security Audit task" -ForegroundColor Red
}
Write-Host

Write-Host "7. Creating Medium Priority Task - UI/UX Improvements" -ForegroundColor Green
$headers = @{ "Authorization" = "Bearer $($UserTokens.PM)" }
$task2 = Invoke-SafeRestMethod -Uri $TASKS_API -Method "Post" -Headers $headers -Body @'
{
  "title": "Redesign User Dashboard",
  "description": "Update the user dashboard with modern UI components, improve accessibility, and enhance user experience. Include responsive design for mobile devices.",
  "assigneeEmail": "sarah.design@taskmanagement.com",
  "dueDate": "2025-08-20T17:00:00Z",
  "priority": 1,
  "tags": ["ui", "ux", "design", "dashboard", "responsive"]
}
'@

if ($task2) {
    Write-Host "✅ Dashboard Redesign task created - ID: $($task2.id)" -ForegroundColor Green
    $task2Id = $task2.id
} else {
    Write-Host "❌ Failed to create Dashboard Redesign task" -ForegroundColor Red
}
Write-Host

Write-Host "8. Creating API Development Task" -ForegroundColor Green
$task3 = Invoke-SafeRestMethod -Uri $TASKS_API -Method "Post" -Headers $headers -Body @'
{
  "title": "Implement REST API Versioning",
  "description": "Add API versioning support to maintain backward compatibility. Implement v1 and v2 endpoints with proper documentation and migration guides.",
  "assigneeEmail": "john.dev@taskmanagement.com",
  "dueDate": "2025-08-25T12:00:00Z",
  "priority": 2,
  "tags": ["api", "versioning", "backend", "documentation"]
}
'@

if ($task3) {
    Write-Host "✅ API Versioning task created - ID: $($task3.id)" -ForegroundColor Green
    $task3Id = $task3.id
} else {
    Write-Host "❌ Failed to create API Versioning task" -ForegroundColor Red
}
Write-Host

Write-Host "9. Creating Database Optimization Task" -ForegroundColor Green
$headers = @{ "Authorization" = "Bearer $($UserTokens.Admin)" }
$task4 = Invoke-SafeRestMethod -Uri $TASKS_API -Method "Post" -Headers $headers -Body @'
{
  "title": "Database Optimization",
  "description": "Optimize database queries and add proper indexing for better performance. Implement caching strategies to improve response times.",
  "assigneeEmail": "john.dev@taskmanagement.com",
  "dueDate": "2025-08-30T16:00:00Z",
  "priority": 2,
  "tags": ["database", "performance", "mongodb", "caching"]
}
'@

if ($task4) {
    Write-Host "✅ Database Optimization task created - ID: $($task4.id)" -ForegroundColor Green
    $task4Id = $task4.id
} else {
    Write-Host "❌ Failed to create Database Optimization task" -ForegroundColor Red
}
Write-Host

Write-Host "10. Creating Automated Testing Task" -ForegroundColor Green
$headers = @{ "Authorization" = "Bearer $($UserTokens.PM)" }
$task5 = Invoke-SafeRestMethod -Uri $TASKS_API -Method "Post" -Headers $headers -Body @'
{
  "title": "Automated Testing Implementation",
  "description": "Set up comprehensive automated testing suite with unit tests, integration tests, and end-to-end tests. Achieve 80%+ code coverage.",
  "assigneeEmail": "mike.qa@taskmanagement.com",
  "dueDate": "2025-09-05T14:00:00Z",
  "priority": 2,
  "tags": ["testing", "automation", "qa", "coverage"]
}
'@

if ($task5) {
    Write-Host "✅ Automated Testing task created - ID: $($task5.id)" -ForegroundColor Green
    $task5Id = $task5.id
} else {
    Write-Host "❌ Failed to create Automated Testing task" -ForegroundColor Red
}
Write-Host

Write-Host "11. Creating Memory Leak Bug Fix Task" -ForegroundColor Green
$headers = @{ "Authorization" = "Bearer $($UserTokens.PM)" }
$task6 = Invoke-SafeRestMethod -Uri $TASKS_API -Method "Post" -Headers $headers -Body @'
{
  "title": "Fix Memory Leak Bug",
  "description": "Investigate and fix memory leak in the threaded comment system affecting performance. This is causing increased RAM usage over time.",
  "assigneeEmail": "john.dev@taskmanagement.com",
  "dueDate": "2025-08-12T09:00:00Z",
  "priority": 3,
  "tags": ["bug", "memory-leak", "performance", "urgent"]
}
'@

if ($task6) {
    Write-Host "✅ Memory Leak Bug Fix task created - ID: $($task6.id)" -ForegroundColor Green
    $task6Id = $task6.id
} else {
    Write-Host "❌ Failed to create Memory Leak Bug Fix task" -ForegroundColor Red
}
Write-Host

# Store task IDs for comment creation
$script:TaskIds = @{
    "SecurityAudit" = $task1Id
    "DashboardRedesign" = $task2Id
    "APIVersioning" = $task3Id
    "DatabaseOptimization" = $task4Id
    "AutomatedTesting" = $task5Id
    "MemoryLeakFix" = $task6Id
}

# Wait a moment before adding comments
Start-Sleep -Seconds 2

Write-Host "Adding realistic threaded comments to tasks..." -ForegroundColor Blue
Write-Host

# Function to add comments with proper error handling
function Add-TaskComment {
    param(
        [string]$TaskId,
        [string]$Content,
        [string]$Token,
        [string]$ParentCommentId = $null
    )
    
    $commentBody = @{
        "content" = $Content
    }
    
    if ($ParentCommentId) {
        $commentBody["parentCommentId"] = $ParentCommentId
    }
    
    $headers = @{ "Authorization" = "Bearer $Token" }
    $response = Invoke-SafeRestMethod -Uri "$TASKS_API/$TaskId/comments" -Method "Post" -Headers $headers -Body ($commentBody | ConvertTo-Json)
    
    if ($response) {
        return $response.comments[-1].id  # Return the ID of the newly created comment
    }
    return $null
}

# 3. Add Comments to Security Audit Task
if ($TaskIds.SecurityAudit) {
    Write-Host "12. Adding comments to Security Audit task" -ForegroundColor Green
    
    $comment1 = Add-TaskComment -TaskId $TaskIds.SecurityAudit -Content "This is a critical task! We need to prioritize this to ensure our application is secure. Please start with the dependency scanning first as it should be the quickest to identify known vulnerabilities." -Token $UserTokens.PM
    
    if ($comment1) {
        Write-Host "   ✅ PM comment added" -ForegroundColor Green
        
        $comment2 = Add-TaskComment -TaskId $TaskIds.SecurityAudit -Content "Good point! I already started with dependency scanning using npm audit and found 3 high-severity vulnerabilities in our frontend packages. Working on updating them now. For the backend, I plan to use OWASP Dependency Check." -Token $UserTokens.Developer -ParentCommentId $comment1
        
        if ($comment2) {
            Write-Host "   ✅ Developer reply added" -ForegroundColor Green
            
            $comment3 = Add-TaskComment -TaskId $TaskIds.SecurityAudit -Content "I can help with the penetration testing part. I have experience with OWASP ZAP and Burp Suite. Should I start setting up test scenarios while you work on the dependencies?" -Token $UserTokens.QA -ParentCommentId $comment2
            
            if ($comment3) {
                Write-Host "   ✅ QA nested reply added" -ForegroundColor Green
            }
        }
    }
}
Write-Host

# 4. Add Comments to Dashboard Redesign Task
if ($TaskIds.DashboardRedesign) {
    Write-Host "13. Adding comments to Dashboard Redesign task" -ForegroundColor Green
    
    $comment1 = Add-TaskComment -TaskId $TaskIds.DashboardRedesign -Content "I have completed the initial wireframes and user flow diagrams. The new design focuses on better data visualization and improved user navigation. I used Figma for the prototypes. Ready for review!" -Token $UserTokens.Designer
    
    if ($comment1) {
        Write-Host "   ✅ Designer comment added" -ForegroundColor Green
        
        $comment2 = Add-TaskComment -TaskId $TaskIds.DashboardRedesign -Content "Great work, Sarah! I reviewed the Figma prototypes and they look fantastic. The new layout is much more intuitive. Can you also include dark mode support? Our users have been requesting this feature." -Token $UserTokens.PM -ParentCommentId $comment1
        
        if ($comment2) {
            Write-Host "   ✅ PM reply added" -ForegroundColor Green
            
            $comment3 = Add-TaskComment -TaskId $TaskIds.DashboardRedesign -Content "I can help implement the frontend components once the designs are finalized. For the dark mode, I suggest using CSS custom properties and a theme context in React. This will make it easy to toggle between themes." -Token $UserTokens.Developer -ParentCommentId $comment2
            
            if ($comment3) {
                Write-Host "   ✅ Developer nested reply added" -ForegroundColor Green
            }
        }
    }
}
Write-Host

# 5. Add Comments to API Versioning Task
if ($TaskIds.APIVersioning) {
    Write-Host "14. Adding comments to API Versioning task" -ForegroundColor Green
    
    $comment1 = Add-TaskComment -TaskId $TaskIds.APIVersioning -Content "This is a critical architectural decision. We need to ensure backward compatibility while introducing v2 endpoints. I recommend using semantic versioning and maintaining v1 for at least 6 months after v2 release." -Token $UserTokens.Admin
    
    if ($comment1) {
        Write-Host "   ✅ Admin comment added" -ForegroundColor Green
        
        $comment2 = Add-TaskComment -TaskId $TaskIds.APIVersioning -Content "Agreed on the 6-month overlap. I plan to implement versioning through URL path (/api/v1/ vs /api/v2/) rather than headers. This makes it more explicit and easier for client developers to understand. Should I start with the authentication endpoints?" -Token $UserTokens.Developer -ParentCommentId $comment1
        
        if ($comment2) {
            Write-Host "   ✅ Developer reply added" -ForegroundColor Green
        }
    }
}
Write-Host

# 6. Add Comments to Database Optimization Task
if ($TaskIds.DatabaseOptimization) {
    Write-Host "15. Adding comments to Database Optimization task" -ForegroundColor Green
    
    $comment1 = Add-TaskComment -TaskId $TaskIds.DatabaseOptimization -Content "I analyzed the MongoDB performance and found several issues: 1) Missing indexes on frequently queried fields, 2) Some queries are not optimized, 3) Connection pooling needs adjustment. I created a plan to address these systematically." -Token $UserTokens.Developer
    
    if ($comment1) {
        Write-Host "   ✅ Developer comment added" -ForegroundColor Green
        
        $comment2 = Add-TaskComment -TaskId $TaskIds.DatabaseOptimization -Content "Excellent analysis! Can you prioritize the indexing work first? That should give us the biggest performance gain with minimal risk. Also, please share the performance benchmarks before and after the optimization." -Token $UserTokens.Admin -ParentCommentId $comment1
        
        if ($comment2) {
            Write-Host "   ✅ Admin reply added" -ForegroundColor Green
        }
    }
}
Write-Host

# 7. Add Comments to Automated Testing Task
if ($TaskIds.AutomatedTesting) {
    Write-Host "16. Adding comments to Automated Testing task" -ForegroundColor Green
    
    $comment1 = Add-TaskComment -TaskId $TaskIds.AutomatedTesting -Content "I have set up the initial test framework using Jest for unit tests and Cypress for end-to-end testing. The test coverage is currently at 65%. I am focusing on testing the critical user flows first - authentication, task CRUD operations, and comment threading." -Token $UserTokens.QA
    
    if ($comment1) {
        Write-Host "   ✅ QA comment added" -ForegroundColor Green
        
        $comment2 = Add-TaskComment -TaskId $TaskIds.AutomatedTesting -Content "Great progress on the test coverage! The comment threading tests are especially important given our recent implementation. Can you also add performance tests to ensure the API responses stay under 200ms for typical operations?" -Token $UserTokens.PM -ParentCommentId $comment1
        
        if ($comment2) {
            Write-Host "   ✅ PM reply added" -ForegroundColor Green
        }
    }
}
Write-Host

# 8. Add Comments to Memory Leak Bug Fix Task
if ($TaskIds.MemoryLeakFix) {
    Write-Host "17. Adding comments to Memory Leak Bug Fix task" -ForegroundColor Green
    
    $comment1 = Add-TaskComment -TaskId $TaskIds.MemoryLeakFix -Content "I reproduced the memory leak issue! It happens after running the application for about 2 hours with continuous task operations. Memory usage grows from 150MB to over 400MB. I created a detailed bug report with steps to reproduce and memory profiling data." -Token $UserTokens.QA
    
    if ($comment1) {
        Write-Host "   ✅ QA comment added" -ForegroundColor Green
        
        $comment2 = Add-TaskComment -TaskId $TaskIds.MemoryLeakFix -Content "Thanks for the detailed report! I suspect the issue is related to event listeners not being properly cleaned up in the comment components. I found several React useEffect hooks missing cleanup functions. Working on a fix now." -Token $UserTokens.Developer -ParentCommentId $comment1
        
        if ($comment2) {
            Write-Host "   ✅ Developer reply added" -ForegroundColor Green
            
            $comment3 = Add-TaskComment -TaskId $TaskIds.MemoryLeakFix -Content "This is critical since it affects our production environment. John, please prioritize this over other tasks. Mike, can you set up continuous memory monitoring to catch similar issues early in the future?" -Token $UserTokens.Admin -ParentCommentId $comment2
            
            if ($comment3) {
                Write-Host "   ✅ Admin nested reply added" -ForegroundColor Green
            }
        }
    }
}
Write-Host

Write-Host

# Final verification - Get all tasks with comments
Write-Host "Getting all tasks to verify creation and comments..." -ForegroundColor Blue
Write-Host

Write-Host "18. Fetching All Tasks with Comments" -ForegroundColor Green
$headers = @{ "Authorization" = "Bearer $($UserTokens.Admin)" }
$allTasks = Invoke-SafeRestMethod -Uri $TASKS_API -Method "Get" -Headers $headers

if ($allTasks) {
    Write-Host "✅ Successfully retrieved all tasks" -ForegroundColor Green
    Write-Host "Total tasks created: $($allTasks.Count)" -ForegroundColor Cyan
    
    foreach ($task in $allTasks) {
        Write-Host "   📋 $($task.title) (ID: $($task.id))" -ForegroundColor White
        Write-Host "      👤 Assigned to: $($task.assigneeEmail)" -ForegroundColor Gray
        Write-Host "      🏷️  Priority: $($task.priorityName) | Status: $($task.statusName)" -ForegroundColor Gray
        Write-Host "      💬 Comments: $($task.comments.Count)" -ForegroundColor Gray
        
        if ($task.comments.Count -gt 0) {
            foreach ($comment in $task.comments) {
                Write-Host "         💭 $($comment.authorName): $($comment.content.Substring(0, [Math]::Min(50, $comment.content.Length)))..." -ForegroundColor DarkGray
                if ($comment.replies.Count -gt 0) {
                    Write-Host "            └─ $($comment.replies.Count) replies" -ForegroundColor DarkGray
                }
            }
        }
        Write-Host
    }
} else {
    Write-Host "❌ Failed to retrieve tasks" -ForegroundColor Red
}

Write-Host "=== Complete Seeding Finished ===" -ForegroundColor Blue
Write-Host
Write-Host "🎉 All data has been successfully seeded!" -ForegroundColor Green
Write-Host
Write-Host "📊 Summary:" -ForegroundColor Cyan
Write-Host "   👥 Users Created: 5 (Admin, PM, Developer, Designer, QA)"
Write-Host "   📋 Tasks Created: 6 (with different priorities and assignees)"
Write-Host "   💬 Comments Added: Realistic threaded conversations on all tasks"
Write-Host "   🔗 Threading Levels: Up to 3 levels deep with natural workplace discussions"
Write-Host
Write-Host "🔐 Login Credentials:" -ForegroundColor Yellow
Write-Host "   👑 Admin: admin@taskmanagement.com (Admin123!)"
Write-Host "   👔 PM: pm@taskmanagement.com (Manager123!)"  
Write-Host "   💻 Developer: john.dev@taskmanagement.com (Dev123!)"
Write-Host "   🎨 Designer: sarah.design@taskmanagement.com (Design123!)"
Write-Host "   🧪 QA: mike.qa@taskmanagement.com (QA123!)"
Write-Host
Write-Host "🚀 Ready for Testing:" -ForegroundColor Green
Write-Host "   🌐 Frontend: http://localhost:3000"
Write-Host "   🔧 API: http://localhost:5000"
Write-Host "   🔑 Auth: http://localhost:5001"
Write-Host
Write-Host "✨ Features to Test:" -ForegroundColor Magenta
Write-Host "   ✅ User authentication and role-based access"
Write-Host "   ✅ Task management (CRUD operations)"
Write-Host "   ✅ Threaded comment system with user avatars"
Write-Host "   ✅ Modern UI with enhanced styling"
Write-Host "   ✅ Real-time comment updates"
Write-Host "   ✅ Task status and priority management"
Write-Host "   ✅ Responsive design and user experience"
Write-Host
Write-Host "🎯 The system is now ready for comprehensive testing and demonstration!" -ForegroundColor Green
