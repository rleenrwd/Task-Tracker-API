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

## 6. TaskItem EditDraft Method

### Requirement

The application must allow a task to be edited while it is in the Draft state.

A submitted task must not be editable, and an edited draft must still contain a valid title. The description may remain null while the task is a draft.

### Implementation

Create a public instance method named `EditDraft(...)` that accepts:

- **title (string):** The updated task title.
- **description (string?):** The updated task description, which may remain null while the task is a draft.

The method performs the following operations:

1. Checks whether the current task's Status is `TaskItemStatus.Draft`.
2. Throws an `InvalidOperationException` if the task is not a draft.
3. Validates the new title using `string.IsNullOrWhiteSpace()`.
4. Throws an `ArgumentException` with `nameof(title)` if the title is invalid.
5. Trims leading and trailing whitespace from the title.
6. Updates the task's Title and Description properties.

### Why This Fits

`EditDraft(...)` is an **instance method** because it modifies the state of an existing TaskItem.

The method uses `this.Status`, `this.Title`, and `this.Description` to work with the specific TaskItem instance that called the method.

Keeping the editing rules inside the entity supports **encapsulation**. Outside code cannot directly modify the private-set properties and bypass the task's business rules.

The lifecycle check happens before any properties are changed. This prevents submitted tasks from being modified.

The title is also validated before either property is updated. This prevents the entity from being left in a partially updated or invalid state if validation fails.

### Alternatives & Tradeoffs

The application service or controller could check whether a task is editable before changing its properties.

However, relying only on outside components would make the business rule easier to bypass. Another part of the application could potentially modify the task without performing the same validation.

Public setters could also allow direct editing, but they would remove the entity's control over when and how its state changes.

The method could return a new TaskItem instead of modifying the existing instance, using an immutable design. However, the current requirements allow drafts to change over time, so controlled mutation through an instance method is a simpler fit.

### What It Does Not Do

The method does not save the changes to the database.

It only changes the TaskItem entity in memory. Persistence will be handled by another application component.

The method also does not allow editing after submission and does not handle task submission itself.

The behavior has compiled successfully but still needs unit tests to verify the editing rules.

### Transferable Knowledge

**Instance methods** are useful when behavior applies to a specific existing object and may change that object's state.

Examples include:

- **Order:** Update shipping information while the order is still editable.
- **Bank Account:** Withdraw money from a specific account while enforcing balance rules.
- **Inventory Item:** Adjust the quantity of a specific inventory record while enforcing stock rules.
- **Investigation Case:** Update case details only while the case is in an editable workflow state.

A broader principle is to **validate an operation before changing state**. Performing all required checks first helps prevent partial updates and keeps an entity valid when an operation fails.

Another transferable principle is that **business rules are harder to bypass when the entity that owns the state also controls how that state can change**.

---

## 7. TaskItem EditDraft Unit Tests

### Requirement

The application must allow existing tasks to be edited while they are in the Draft state.

Editing must enforce the task's business rules, including requiring a valid title, allowing an incomplete description, and preventing changes after submission.

Automated tests must verify that valid edits succeed, invalid edits are rejected, and rejected operations preserve the task's existing state.

### Implementation

Create **six xUnit unit tests** within the `TaskItemTests` class to verify the `EditDraft(...)` method:

1. **EditDraft_WithValidInputs_UpdatesProperties:** Verifies that a draft's title and description can be updated while its status remains Draft.
2. **EditDraft_WithEmptyTitle_ThrowsArgumentException:** Verifies that an empty title is rejected and the original title remains unchanged.
3. **EditDraft_WithWhitespaceTitle_ThrowsArgumentException:** Verifies that a whitespace-only title is rejected and the original title remains unchanged.
4. **EditDraft_WithNullTitle_ThrowsArgumentException:** Verifies that a null title is rejected and the original title remains unchanged.
5. **EditDraft_WithPaddedTitle_TrimsWhitespace:** Verifies that leading and trailing whitespace is removed from an updated title.
6. **EditDraft_WithNullDescription_UpdatesDescription:** Verifies that a draft's description can be changed to null while the task remains in Draft status.

An additional test verifies the lifecycle restriction:

7. **EditDraft_WhenAlreadySubmitted_ThrowsInvalidOperationException:** Verifies that a submitted task cannot be edited and that its original title remains unchanged.

The tests use several **xUnit assertions**:

- **Assert.Equal():** Verifies that actual property values match the expected values.
- **Assert.Null():** Verifies that the description can be cleared during draft editing.
- **Assert.Throws<T>():** Verifies that invalid editing operations throw the expected exception type.

The tests follow the **Arrange, Act, Assert (AAA)** pattern:

- **Arrange:** Create a valid TaskItem and prepare the updated values.
- **Act:** Call `EditDraft(...)` with the updated information.
- **Assert:** Verify that the resulting behavior matches the requirement.

### Why This Fits

**Unit testing** verifies that the `EditDraft(...)` method correctly enforces the business rules governing changes to an existing task.

Testing successful edits confirms that the method updates the entity's properties without changing its lifecycle state.

Testing invalid titles separately verifies that the method rejects null, empty, and whitespace-only values.

**Exception assertions** verify that invalid operations are rejected rather than silently accepted.

Additional assertions verify that the original title remains unchanged after a failed edit.

This reinforces an important design principle: **validation should occur before modifying an entity's state**.

Testing title trimming also verifies that input normalization works during editing, not only during initial creation.

The submitted-task test confirms that the entity enforces its own lifecycle restrictions through the `EditDraft(...)` method.

### Alternatives & Tradeoffs

**Manual testing** could verify whether draft editing behaves correctly, but repeating these checks after every code change would be inefficient and prone to human error.

**Parameterized tests using [Theory] and [InlineData]** could combine the null, empty, and whitespace-only title scenarios into a single test method.

This would reduce repeated code. However, separate `[Fact]` tests make each validation scenario explicit and easier to understand while learning xUnit.

**Integration tests** could verify editing through the database or API, but these would introduce additional dependencies that are unnecessary for testing the entity's current business rules.

Unit tests are appropriate because `EditDraft(...)` modifies an existing object directly in memory without requiring a database or external service.

The chosen approach provides **focused behavioral verification, clear test failures, and minimal testing dependencies**.

### What It Does Not Do

These tests verify the behavior of `EditDraft(...)` but do not confirm that edited tasks are persisted to SQL Server.

They also do not verify that editing operations are correctly exposed through application services or HTTP endpoints.

The tests do not currently verify every possible invalid input or every property after every rejected operation.

For example, the submitted-task test attempts to change the title and verifies that the original title remains unchanged. It does not separately attempt to change the description.

The tests also do not verify database concurrency, where multiple operations attempt to modify the same task simultaneously.

These concerns will be addressed through application, persistence, and integration testing where appropriate.

### Transferable Knowledge

**Unit testing state-changing methods** is important when objects contain business rules that control how their properties can be modified.

Examples include:

- **Bank Account:** Verify that an invalid withdrawal is rejected without changing the account balance.
- **Order:** Verify that an order can be edited before fulfillment but cannot be modified after shipment.
- **Inventory Item:** Verify that invalid quantity adjustments do not change the existing stock level.
- **Investigation Case:** Verify that case information cannot be modified when its lifecycle state prohibits editing.

**Testing invalid operations** is just as important as testing successful operations. A method must not only perform valid changes correctly but also reject changes that violate business rules.

**State preservation** is another transferable concept. When an operation fails validation, the entity should retain its previously valid state rather than becoming partially modified.

**Lifecycle-based validation** allows the same operation to behave differently depending on the entity's current state.

The broader principle is that **unit tests should verify both what an entity is allowed to do and what it must prevent, ensuring that state changes follow the application's business rules**.

---

## 8. TaskItem Submit Method

### Requirement

The application must allow a task to transition from Draft to Submitted only when all required information is present.

A task must have a valid title and description before submission. Once submitted, its status must change to Submitted, and the submission timestamp must be recorded.

A task that has already been submitted cannot be submitted again.

### Implementation

Create a **public instance method** named `Submit(...)` that returns void and accepts one parameter:

- **submittedAt (DateTimeOffset):** The timestamp supplied by the calling application code.

The method performs the following operations:

1. Checks whether the task's current status is Draft.
2. Throws an `InvalidOperationException` if the task is not a draft.
3. Validates the existing title using `string.IsNullOrWhiteSpace()`.
4. Validates the existing description using `string.IsNullOrWhiteSpace()`.
5. Throws an `InvalidOperationException` if either required field is invalid.
6. Changes the task's status to `TaskItemStatus.Submitted`.
7. Assigns the supplied timestamp to `SubmittedAt`.

### Why This Fits

Using a **public instance method** allows the entity to manage the lifecycle of an existing TaskItem.

The method accesses the task's current properties through the `this` keyword rather than requiring those values as parameters.

This supports **encapsulation** because the entity controls its own state transitions and prevents outside code from changing its status arbitrarily.

All validation occurs **before modifying the entity's state**. If validation fails, the task remains unchanged.

The method uses `InvalidOperationException` because submission is being rejected due to the task's current state or incomplete information.

The submission timestamp is passed into the method rather than generated internally using `DateTimeOffset.UtcNow`. This supports **designing for testability**, allowing automated tests to supply a fixed timestamp and verify that the correct value is stored.

### Alternatives & Tradeoffs

The **controller or application service** could handle submission validation, but this would move the entity's business rules outside the entity itself. Other components could potentially bypass those checks.

The method could also accept **title and description parameters**, but these values already exist within the TaskItem. Passing them again would be unnecessary because submission validates existing information rather than updating it.

Another option would be to generate the submission timestamp internally using `DateTimeOffset.UtcNow`. This would simplify the method signature but make testing exact timestamp values less predictable.

The chosen design keeps **business rules within the entity, avoids unnecessary parameters, and supports predictable testing**.

### What It Does Not Do

The method does not save the submitted task to the database. It only updates the existing entity's state in memory.

It does not allow editing task information during submission. Any required changes must be made while the task is still a draft.

It also does not support reversing a submission or reopening a submitted task.

The submission behavior still requires unit tests to verify successful transitions, rejected submissions, and timestamp initialization.

### Transferable Knowledge

**State transitions** are useful when an entity moves through different stages of a business process.

Examples include:

- **Order:** Transition from Pending to Confirmed only when required order information is complete.
- **Payment:** Transition from Pending to Completed only after successful payment processing.
- **Leave Request:** Transition from Draft to Submitted only when required information is present.
- **Investigation Case:** Transition between workflow states only when the required business conditions are satisfied.

**Encapsulation** allows entities to control these transitions rather than exposing their state for unrestricted modification.

**Validating before modifying state** helps ensure that failed operations do not leave entities partially updated.

**Designing for testability** allows methods to receive controlled values, such as timestamps, making their behavior easier to verify.

The broader principle is that **an entity should enforce the business rules governing its lifecycle and only permit state changes when the required conditions are satisfied**.

---

## 9. TaskItem Submit Unit Tests

### Requirement

The application must allow a valid draft to transition to the Submitted state only when all required information is present.

Submission requires a valid title and description. Once submitted, the task must record its submission timestamp and prevent additional submissions.

Automated tests must verify that valid submissions succeed, invalid submissions are rejected, and failed operations preserve the task's existing lifecycle state.

### Implementation

Create **five xUnit unit tests** within the `TaskItemTests` class to verify the `Submit(...)` method:

1. **Submit_WithValidDraft_UpdatesStatusAndTimestamp:** Verifies that a valid draft transitions to Submitted and records the supplied submission timestamp.
2. **Submit_WithNullDescription_ThrowsInvalidOperationException:** Verifies that submission is rejected when the description is null.
3. **Submit_WithEmptyDescription_ThrowsInvalidOperationException:** Verifies that submission is rejected when the description is empty.
4. **Submit_WithWhitespaceDescription_ThrowsInvalidOperationException:** Verifies that submission is rejected when the description contains only whitespace.
5. **Submit_WhenAlreadySubmitted_ThrowsInvalidOperationException:** Verifies that a submitted task cannot be submitted again and that its original submission timestamp is preserved.

The tests use several **xUnit assertions**:

- **Assert.Equal():** Verifies that the task's status and submission timestamp match the expected values.
- **Assert.Null():** Verifies that rejected submissions do not assign a submission timestamp.
- **Assert.Throws<T>():** Verifies that invalid submission attempts throw the expected exception type.

The tests follow the **Arrange, Act, Assert (AAA)** pattern:

- **Arrange:** Create a TaskItem with the necessary information and define a submission timestamp.
- **Act:** Call `Submit(...)` on the existing task.
- **Assert:** Verify that the operation produces the expected state changes or rejects the submission.

### Why This Fits

**Unit testing** verifies that the `Submit(...)` method correctly enforces the business rules governing the transition from Draft to Submitted.

The successful submission test confirms that the entity changes its lifecycle state and records the supplied timestamp.

Using a **fixed DateTimeOffset** allows the test to verify the exact timestamp stored by the entity. This reinforces the earlier design decision to pass timestamps into methods rather than retrieving the current time internally.

Testing null, empty, and whitespace-only descriptions separately verifies that submission requires meaningful information rather than simply accepting any string value.

**Exception assertions** confirm that invalid submissions are rejected through an `InvalidOperationException`.

Additional assertions verify that rejected submissions leave the task in Draft status with SubmittedAt remaining null.

The duplicate-submission test verifies that an already submitted task cannot transition again or overwrite its original submission timestamp.

Together, these tests reinforce **lifecycle validation, state preservation, and predictable testing**.

### Alternatives & Tradeoffs

**Manual testing** could verify submission behavior, but automated unit tests provide repeatable checks that can be executed after future code changes.

**Parameterized tests using [Theory] and [InlineData]** could combine the null, empty, and whitespace-only description scenarios into one test method.

This would reduce repeated test code. However, separate `[Fact]` tests were chosen to keep each validation scenario explicit and easier to understand.

**Integration tests** could verify submission through SQL Server or an HTTP endpoint, but these introduce additional dependencies that are unnecessary for testing the entity's lifecycle rules.

The submission timestamp could also be generated using **DateTimeOffset.UtcNow** inside the entity. However, passing a fixed timestamp into `Submit(...)` makes exact timestamp assertions predictable.

The chosen approach provides **focused verification of business rules, clear test failures, and minimal external dependencies**.

### What It Does Not Do

These tests verify the in-memory behavior of `Submit(...)`, but they do not confirm that submitted tasks are persisted to SQL Server.

They also do not verify that application services or API endpoints correctly coordinate task submission.

The tests do not currently verify every possible invalid condition.

For example, the Title property is validated during submission, but the submission tests do not separately attempt to submit a task with an invalid title. The controlled creation and editing methods already reject those inputs through their own validation rules.

The tests also do not verify chronological ordering between CreatedAt and SubmittedAt. The current method does not enforce a rule requiring the submission timestamp to occur after the creation timestamp.

Database persistence, application orchestration, HTTP behavior, and additional integrity requirements will be addressed in later development stages where appropriate.

### Transferable Knowledge

**Unit testing lifecycle transitions** is useful when an entity moves between business states that have different requirements and restrictions.

Examples include:

- **Order:** Verify that an order cannot transition to Confirmed without required customer and order information.
- **Payment:** Verify that a completed payment cannot be processed again or have its completion timestamp overwritten.
- **Leave Request:** Verify that a request cannot be submitted without required information and cannot be submitted twice.
- **Investigation Case:** Verify that a case cannot transition to a review or closure state until the required information is complete.

**State preservation** is important when testing invalid operations. Rejected transitions should leave the entity's previously valid state unchanged.

**Testing both successful and rejected transitions** helps verify that an entity permits valid operations while preventing actions that violate business rules.

**Designing for testability** allows external values, such as timestamps, to be controlled during automated testing so the resulting behavior can be verified reliably.

The broader principle is that **lifecycle unit tests should verify not only that valid state transitions succeed, but also that invalid transitions are rejected without altering the entity's existing state**.

---