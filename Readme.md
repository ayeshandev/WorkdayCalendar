# Workday Calendar

## Overview

A robust C# library that calculates workdays while accounting for weekends and holidays. It provides precise datetime calculations for business day arithmetic, supporting both forward and backward increments with fractional workday precision.

**Target Framework**: .NET 10 | **C# Version**: 14.0

## Features

- ✅ **Workday Calculations**: Add or subtract any number of working days (including fractional workdays) from a given datetime
- ✅ **Holiday Support**: Register both single-instance and recurring holidays
- ✅ **Flexible Working Hours**: Define custom business hours (default: 08:00–16:00)
- ✅ **Weekend Handling**: Automatically skips Saturdays and Sundays
- ✅ **Bidirectional**: Supports both forward and backward calculations
- ✅ **Precision**: Handles time within working hours accurately at the minute level
- ✅ **Clean Architecture**: Domain-driven design with SOLID principles

## Core Concepts

### Workday Definition
A workday is defined as:
- A day from **Monday to Friday** (not a weekend)
- **Not registered as a holiday** (neither single nor recurring)
- A time within the configured **working hours**

### Time Normalization
Input datetimes are automatically normalized to valid working time:

**For forward calculations:**
- If before working hours → starts at working day start
- If after working hours → moves to next workday's start
- If on weekend/holiday → moves to next workday's start

**For backward calculations:**
- If before working hours → moves to previous workday's end
- If after working hours → ends at working day end
- If on weekend/holiday → moves to previous workday's end

## Implementation Approach

### 1. **Domain Models & Architecture**
```
WorkingHours        : Value object for business hour ranges
IHoliday (interface): Abstraction for different holiday types
├─ SingleHoliday    : Holiday on a specific date
└─ RecurringHoliday : Holiday recurring annually
WorkdayCalendar     : Main orchestrator (IWorkdayCalendar)
```

**Design Principles Applied:**
- **Single Responsibility**: Each class has one clear concern
- **Open/Closed**: New holiday types can be added without modifying core logic
- **Dependency Inversion**: Core logic depends on `IHoliday` abstraction
- **Domain-Driven Design**: Holiday logic encapsulated in domain models

### 2. **Holiday Management** 
- `SetRecurringHoliday(month, day)`: Register annual holidays (e.g., Christmas, Independence Day)
  - Validates month (1-12) and valid day ranges
  - Prevents February 29 (not recurring every year)
  - Factory method `RecurringHoliday.Create()` handles validation
- `SetSingleHoliday(date)`: Register one-time holidays (e.g., special events)
- Holidays stored in `HashSet<IHoliday>` for O(1) lookup with polymorphic checks

### 3. **Workday Calculation Algorithm**

**Key Steps:**

1. **Determine Direction**: Check if workdays value is positive (forward) or negative (backward)

2. **Normalize Input DateTime**: Convert to a valid effective working time
   - Uses `GetForwardEffectiveDateTime()` or `GetBackwardEffectiveDateTime()`
   - Ensures we start calculations from valid working time

3. **Convert to Minutes**: 
   - Calculate offset from normalized position
   - Convert workday increment to total minutes
   - Sum to get total minutes to distribute

4. **Split into Full Days & Remainder**:
   - Full workdays = `total_minutes / working_hours_total`
   - Remaining minutes = `total_minutes % working_hours_total`

5. **Navigate Calendar**: 
   - `MoveWorkingDays()` traverses the calendar skipping weekends/holidays
   - Distributes remaining minutes within the final workday

6. **Reconstruct DateTime**: 
   - Final time = `Start + remaining_minutes` (forward)
   - Final time = `End - remaining_minutes` (backward)

**Example**: `24-05-2004 18:05 + (-5.5) workdays = 14-05-2004 12:00`
- Input normalized (backward): 18:05 > 16:00 → snapped to 24-05-2004 16:00
- Offset from end: 0 min; increment: 5.5 × 480 = 2,640 min total
- Full workdays back: 5; remaining: 240 min (4 hours)
- Move back 5 workdays from May 24, skipping May 17 (recurring holiday) → lands on May 14
- Result: 16:00 − 4 h = 12:00 → 14-05-2004 12:00 ✓

### 4. **Testable Code Structure**
- Private helper methods for unit testing key behaviors
- Clear separation of concerns allows isolated testing
- Test coverage for boundary conditions (before/after hours, weekends, holidays)
- Floating-point precision handled via truncation to minute level

## API Usage

```csharp
var calendar = new WorkdayCalendar();

// Configure working hours (default: 08:00–16:00)
calendar.SetWorkdayStartAndEnd(new TimeOnly(8, 0), new TimeOnly(16, 0));

// Register holidays
calendar.SetRecurringHoliday(5, 17);           // May 17 every year
calendar.SetSingleHoliday(new DateOnly(2004, 5, 27)); // May 27, 2004 only

// Calculate workdays
var startDate = new DateTime(2004, 5, 24, 18, 5, 0);
var result = calendar.GetWorkdayIncrement(startDate, -5.5m);
// Result: 14-05-2004 12:00

// Forward calculation
var futureDate = calendar.GetWorkdayIncrement(startDate, 44.723656m);
// Result: 27-07-2004 13:47
```

## Design Decisions & Rationale

| Decision | Rationale |
|----------|-----------|
| **`record` for `WorkingHours`** | Immutable value object with automatic equality; suitable for configuration |
| **`IHoliday` interface** | Enables polymorphic behavior; easy to extend with new holiday types |
| **`HashSet<IHoliday>`** | O(1) lookup performance; no duplicates via structural equality |
| **Minute-level precision** | Sufficient for business context; avoids floating-point drift with truncation |
| **Normalization separate from calculation** | Clear separation of concerns; easier to test and reason about |
| **Factory method `Create()` for validation** | Ensures all `RecurringHoliday` instances are valid; domain logic in domain model |

## Testing

The solution includes comprehensive tests covering:
- ✅ Spec examples from requirements
- ✅ Forward and backward calculations
- ✅ Fractional workday handling
- ✅ Single and recurring holiday skipping
- ✅ Time normalization edge cases
- ✅ Boundary conditions (before/after hours, weekend transitions)

Run tests with:
```bash
dotnet test
```

## Code Quality Highlights

- **Clean Code**: Clear naming, minimal complexity, self-documenting logic
- **SOLID Principles**: Single responsibility, open/closed, interface segregation, dependency inversion
- **Domain-Driven Design**: Rich domain models with behavior encapsulation
- **Type Safety**: Leverages C# 14 features and strong typing
- **Documentation**: XML comments on all public APIs; inline comments for complex logic
