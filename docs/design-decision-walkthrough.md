# Task Tracker API - Design Decision Walkthrough

## 1. TaskStatus Enum

### Requirement

The application must distinguish between Draft and Submitted states for a task.

### Implementation

Create a TaskStatus enum with two allowed values:

- Draft
- Submitted

### Why This Fits

An enum is a good choice here because it defines a set of named constants, which helps to prevent inconsistent values from being used. This allows us to represent the task's status using the two defined options: Draft and Submitted.

### Alternatives & Tradeoffs

We could use a string, but strings allow typos and invalid values and provide less compile-time protection.

We could also use separate booleans such as IsDraft and IsSubmitted, but that could create contradictory states such as both being true.

Another choice could've been a Struct, but it would introduce unnecessary complexity that's not needed for this requirement.

An enum is the better choice because our requirement defines only two task states, Draft and Submitted, and an enum provides a simple, type-safe way to represent those predefined states.

### What It Does Not Do

The enum only defines which status values exist. It does not enforce lifecycle rules such as whether a task is allowed to move from Draft to Submitted, etc. The TaskItem entity will be responsible for enforcing these lifecycle rules.

### Transferable Knowledge

Enums are useful whenever a concept has a limited, known set of valid choices. 

Examples include:

- Order Status: Pending, Paid, Shipped, Cancelled
- Payment Status: Pending, Succeeded, Failed
- Approval Status: Draft, Submitted, Approved, Rejected