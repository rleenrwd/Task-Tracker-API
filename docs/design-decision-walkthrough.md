# Task Tracker API - Design Decision Walkthrough

## 1. TaskItemStatus Enum

### Requirement

The application must distinguish between Draft and Submitted states for a task.

### Implementation

Create a TaskItemStatus enum with two defined values:

- **Draft**
- **Submitted**

### Why This Fits

An **enum** is a good choice because it defines a set of named constants, which helps prevent inconsistent values from being used. This allows the application's task status to be represented using the two defined options: Draft and Submitted.

### Alternatives & Tradeoffs

A **string** could be used, but strings allow typos and invalid values and provide less compile-time protection.

Separate **booleans**, such as IsDraft and IsSubmitted, could also be used, but that could create contradictory states, such as both being true.

Another option would be a **struct**, but it would introduce unnecessary complexity that is not needed for this requirement.

An enum is the better choice because the requirement defines only two task states, Draft and Submitted, and an enum provides a simple, type-safe way to represent those predefined states.

### What It Does Not Do

The enum defines the named status values but does not enforce lifecycle rules, such as whether a task is allowed to move from Draft to Submitted or return to Draft after submission. The TaskItem entity will be responsible for enforcing these lifecycle rules.

### Transferable Knowledge

Enums are useful whenever a concept has a limited, known set of valid choices.

Examples include:

- **Order Status:** Pending, Paid, Shipped, Cancelled
- **Payment Status:** Pending, Succeeded, Failed
- **Approval Status:** Draft, Submitted, Approved, Rejected

---

## 2. TaskItem Entity Properties

### Requirement

The application must maintain the information needed to identify a task, describe it, and track its lifecycle while preventing unauthorized changes to its state.

### Implementation

Create a **sealed TaskItem class** with six properties:

- **Id (Guid):** Uniquely identifies each task.
- **Title (string):** Stores the task's title. Initialized to `string.Empty`.
- **Description (string?):** Stores the task's description. It may be null while the task is a draft, but a description is required before the task can be submitted.
- **Status (TaskItemStatus):** Represents the task's current lifecycle state.
- **CreatedAt (DateTimeOffset):** Records when the task was created.
- **SubmittedAt (DateTimeOffset?):** Records when the task was submitted, or remains null until submission.

Each property has a **public getter and private setter**.

### Why This Fits

Using a **class** allows a task to be represented as an entity containing both its data and the behavior that manages that data.

**Private setters** support encapsulation by allowing other components to read the properties without directly modifying them. This helps protect the entity's state so that changes can be controlled through methods that enforce business rules.

The class is **sealed** because the application's requirements do not call for other classes to inherit from TaskItem.

The **nullable properties** allow information that may not yet exist to be represented, such as a description for an incomplete draft or a submission timestamp for a task that has not been submitted.

### Alternatives & Tradeoffs

**Public setters** could be used, but they would allow outside code to modify properties such as Status and SubmittedAt without following the task's business rules.

The class could remain **unsealed**, allowing inheritance. However, the current requirements do not call for specialized task subclasses.

Another option would be to make every property **immutable**, but the requirements specify that drafts must be editable. Private setters provide controlled mutability.

The chosen design provides a balance between **encapsulation, flexibility, and simplicity**.

### What It Does Not Do

The properties alone do not enforce the task's business rules.

For example, initializing Title to `string.Empty` satisfies C#'s non-nullability requirement, but it does not guarantee that a task has a valid, non-empty title.

Controlled creation and modification methods are still needed to validate task information and enforce lifecycle rules.

### Transferable Knowledge

**Encapsulation** is useful when objects must protect their internal state and prevent arbitrary modifications.

Examples include:

- **Bank Account:** Prevent outside code from directly changing the account balance without a valid transaction.
- **Order:** Prevent changes to an order's status without following its fulfillment rules.
- **Inventory Item:** Control updates to available quantities to prevent invalid inventory levels.

---

## 3. TaskItem Private Constructor

### Requirement

The application must initialize newly created tasks with the correct information and lifecycle state. Every new task must begin as a Draft, with no submission timestamp.

### Implementation

Create a **private parameterized constructor** that accepts four parameters:

- **id (Guid):** The task's unique identifier.
- **title (string):** The task's title.
- **description (string?):** The task's description, which may be null while the task is a draft.
- **createdAt (DateTimeOffset):** The task's creation timestamp.

The constructor assigns these values to their corresponding properties and automatically initializes:

- **Status:** `TaskItemStatus.Draft`
- **SubmittedAt:** `null`

### Why This Fits

A **private constructor** prevents outside code from directly creating TaskItem instances using `new TaskItem(...)`. Instead, task creation will be controlled through a public static factory method named `Create(...)`.

Using a **parameterized constructor** centralizes property initialization. Each time the constructor executes, it initializes the task's properties in one place rather than spreading those assignments across multiple methods.

The constructor also sets Status to Draft and SubmittedAt to null because these values are determined by the application's business requirements. They do not need to be supplied as parameters.

This separates responsibilities between the constructor and factory method:

- **Constructor:** Initializes the entity's properties.
- **Factory Method:** Validates creation requirements, generates the ID, and calls the constructor.

### Alternatives & Tradeoffs

A **private parameterless constructor** could also be used, with the factory method initializing every property through an object initializer.

This approach would work, but it would place both validation and property initialization inside the factory method. The constructor itself would not establish the entity's required initial state.

Another option would be a **public parameterized constructor**, but this would allow outside code to instantiate TaskItem directly, bypassing the planned factory method and its validation rules.

The chosen design provides controlled creation, centralized initialization, and a clear separation of responsibilities.

### What It Does Not Do

The constructor does not validate the values passed into it.

For example, it assigns the title but does not check whether the title is null, empty, or whitespace.

The upcoming `Create(...)` factory method will enforce these requirements before calling the constructor.

The constructor also does not save the entity to the database. Persistence will be handled by other application components.

### Transferable Knowledge

**Private parameterized constructors** are useful when domain entities require controlled creation and consistent initialization.

Examples include:

- **Order:** Initialize an order with an identifier, customer information, and an initial Pending status.
- **Bank Account:** Initialize an account with an identifier, owner, and starting balance.
- **Investigation Case:** Initialize a case with its identifier, creation timestamp, and initial workflow status.

Combining a **private constructor with a public static factory method** is a reusable design pattern that separates validation and creation decisions from property initialization.

The broader principle is that **constructors establish an object's initial state, while factory methods can control how and under what conditions an object is created**.

---

## 4. TaskItem Create Factory Method

### Requirement

The application must allow tasks to be created as drafts. Every new task must have a valid title, while the description can remain incomplete until submission.

Each new task must receive a unique identifier, a creation timestamp, and the correct initial lifecycle state.

### Implementation

Create a **public static factory method** named `Create(...)` that returns a `TaskItem`.

The method accepts three parameters:

- **title (string):** The required title of the task.
- **description (string?):** The task's description, which may be null while the task is a draft.
- **createdAt (DateTimeOffset):** The creation timestamp supplied by the calling application code.

The method performs the following operations:

1. Validates the title using `string.IsNullOrWhiteSpace()`.
2. Throws an `ArgumentException` with `nameof(title)` if the title is invalid.
3. Generates a unique identifier using `Guid.NewGuid()`.
4. Removes leading and trailing whitespace from the title using `Trim()`.
5. Calls the private constructor using `new TaskItem(...)`.
6. Returns the initialized TaskItem directly to the caller.

The private constructor initializes the properties, including setting Status to Draft and SubmittedAt to null.

### Why This Fits

A **static factory method** provides a controlled way to create new instances of a class without requiring an existing object.

Because the constructor is private, outside code cannot directly instantiate TaskItem using `new TaskItem(...)`. Instead, it must use the public `Create(...)` method.

This supports **encapsulation** by ensuring that normal task creation goes through the required validation before an object is returned.

The factory method handles creation-related decisions, while the constructor handles property initialization.

The creation timestamp is passed into the factory rather than retrieved internally using `DateTimeOffset.UtcNow`. This supports **designing for testability**, because automated tests can supply a fixed timestamp and verify that the entity stores the expected value.

Returning `new TaskItem(...)` directly also avoids introducing an unnecessary local variable when no additional operations are needed before returning the object.

### Alternatives & Tradeoffs

A **public constructor** could accept the required values directly, but this would place validation and initialization together in the constructor and eliminate the controlled factory entry point.

A **private parameterless constructor with an object initializer** could also be used. The factory would create an instance and assign its properties directly. However, the constructor would no longer centralize initialization, and the factory would take on that additional responsibility.

The entity could generate its own creation timestamp using **DateTimeOffset.UtcNow**, but automated tests would have less control over the exact timestamp being stored. Passing the timestamp as an argument allows predictable testing.

The factory could also assign the new object to a local variable before returning it. However, directly returning `new TaskItem(...)` is simpler when no additional operations are required.

The chosen approach provides **controlled creation, clear responsibilities, predictable testing, and readable code**.

### What It Does Not Do

The factory method creates a valid draft entity in memory, but it does not save the task to the database.

Database persistence will be handled by other application components.

The method also does not submit a task or enforce submission requirements. A description is allowed to be null during creation, but it must contain valid information before submission.

Although the method rejects invalid titles, the Domain project has not yet been behaviorally tested. A successful build confirms compilation, not that the business rules behave correctly.

### Transferable Knowledge

**Static factory methods** are useful when object creation must follow specific business rules or requires additional operations before an instance is returned.

Examples include:

- **Order:** Validate required order information before creating an order in the Pending state.
- **Bank Account:** Validate account information and create an account with a unique identifier and initial balance.
- **Investigation Event:** Validate required event information before creating a draft event.
- **Leave Request:** Validate employee and request information before initializing a new request.

The combination of a **private constructor and public static factory method** allows an entity to control how new instances are created while separating creation rules from property initialization.

**Designing for testability** is another transferable principle. Passing required values, such as timestamps, into methods allows automated tests to control those values and verify behavior predictably.

The broader lesson is that **object creation can be treated as a controlled business operation rather than simply allowing any caller to instantiate an object with arbitrary values**.

---

## 5. TaskItem Creation Unit Tests

### Requirement

The application must correctly create draft tasks, initialize their properties, and reject invalid input.

Automated tests must verify that the `TaskItem.Create(...)` factory method behaves according to the established business requirements.

### Implementation

Create an **xUnit test class** named `TaskItemTests` containing eight unit tests:

1. **Create_WithValidTitle_ReturnsDraftTask:** Verifies that a new task starts in the Draft state.
2. **Create_WithValidInputs_InitializesProperties:** Verifies that the task's properties are initialized correctly.
3. **Create_WithEmptyTitle_ThrowsArgumentException:** Verifies that an empty title is rejected.
4. **Create_WithWhitespaceTitle_ThrowsArgumentException:** Verifies that a whitespace-only title is rejected.
5. **Create_WithNullTitle_ThrowsArgumentException:** Verifies that a null title is rejected.
6. **Create_WithPaddedTitle_TrimsWhitespace:** Verifies that leading and trailing whitespace is removed from a valid title.
7. **Create_WithNullDescription_ReturnsDraftTask:** Verifies that a draft can be created without a description.
8. **Create_MultipleTasks_GeneratesUniqueIds:** Verifies that two separately created tasks receive different identifiers.

The tests use several **xUnit assertions**:

- **Assert.Equal():** Verifies that actual and expected values match.
- **Assert.NotEqual():** Verifies that two values are different.
- **Assert.Null():** Verifies that a value is null.
- **Assert.Throws<T>():** Verifies that an operation throws the expected exception type.

The tests follow the **Arrange, Act, Assert (AAA)** pattern:

- **Arrange:** Prepare the inputs needed for the test.
- **Act:** Execute the operation being tested.
- **Assert:** Verify that the result matches the expected behavior.

### Why This Fits

**Unit testing** allows individual components to be tested independently, helping verify that business rules work as intended.

The `TaskItem.Create(...)` method contains several responsibilities that must behave correctly, including title validation, identifier generation, and task initialization.

Testing these behaviors separately makes it easier to identify which requirement is failing when a test does not pass.

The **[Fact] attribute** identifies each method as an xUnit test that should be executed by the test runner.

**Assert.Throws<ArgumentException>()** uses a lambda expression to pass the operation being tested into the assertion. This allows xUnit to execute the operation and verify that it throws the expected exception.

A fixed `DateTimeOffset` is used when verifying timestamp initialization. This makes the test predictable because the expected timestamp is known beforehand.

The tests also demonstrate the importance of testing actual behavior rather than simply checking whether code executes successfully.

### Alternatives & Tradeoffs

**Manual testing** could be used to verify task creation, but repeating the same checks after every code change would be time-consuming and prone to human error.

**Parameterized tests using [Theory] and [InlineData]** could combine the empty, null, and whitespace-only title scenarios into one test method.

This would reduce repeated test code. However, separate `[Fact]` tests were chosen to make each validation scenario easier to understand while learning the testing framework.

**Integration tests** could verify task creation through the database, but they would introduce additional dependencies and complexity that are unnecessary for testing the entity's current business rules.

Unit tests are the appropriate starting point because the creation logic can be executed directly without requiring an API or database.

### What It Does Not Do

The eight tests verify the implemented task-creation behaviors, but they do not test the complete application.

They do not verify:

- **Database persistence:** Whether tasks are successfully saved to and retrieved from SQL Server.
- **HTTP behavior:** Whether API endpoints correctly process requests and return responses.
- **Draft editing:** Whether existing drafts can be modified according to business rules.
- **Submission:** Whether incomplete drafts are rejected and valid drafts transition to Submitted.
- **Full identifier uniqueness:** Testing two generated IDs does not mathematically guarantee that collisions can never occur.

The tests also do not currently verify the exception's parameter name or every possible invalid input.

These additional behaviors will be addressed through future unit and integration tests where appropriate.

### Transferable Knowledge

**Unit testing** is useful for verifying individual methods, classes, and business rules without requiring the entire application to run.

Examples include:

- **Bank Account:** Verify that withdrawing more than the available balance is rejected.
- **Order:** Verify that an order cannot be submitted without required information.
- **Inventory Item:** Verify that an operation cannot reduce available stock below zero.
- **Investigation Case:** Verify that invalid status transitions are rejected.
- **User Account:** Verify that required information is validated during account creation.

The **Arrange, Act, Assert pattern** provides a reusable structure for organizing tests and making their purpose clear.

**Exception testing** is useful when business rules require invalid operations to be rejected rather than silently accepted.

**Designing for testability** is also an important engineering principle. Providing controlled inputs, such as fixed timestamps, makes it easier to verify behavior reliably.

The broader lesson is that **automated testing should verify requirements and expected behavior, not simply whether code compiles or runs without errors**.

---