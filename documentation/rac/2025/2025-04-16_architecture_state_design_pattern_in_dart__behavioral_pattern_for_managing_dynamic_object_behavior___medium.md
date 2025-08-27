```yaml
---
title: "State Design Pattern in Dart: Behavioral Pattern for Managing Dynamic Object Behavior | Medium"
source: https://maxim-gorin.medium.com/stop-writing-if-else-trees-use-the-state-pattern-instead-1fe9ff39a39c
date_published: 2025-04-16T05:11:29.955Z
date_captured: 2025-08-08T18:26:07.719Z
domain: maxim-gorin.medium.com
author: Maxim Gorin
category: architecture
technologies: [Dart, UML]
programming_languages: [Dart]
tags: [design-patterns, state-pattern, software-design, object-oriented-programming, refactoring, conditional-logic, polymorphism, dart, behavioral-pattern, clean-code]
key_concepts: [State design pattern, Context (design pattern), State Interface (design pattern), Concrete State Classes (design pattern), State Transitions, Conditional complexity, Polymorphism, Open/Closed Principle]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the State design pattern, a behavioral software pattern that allows an object to alter its behavior based on its internal state, thereby eliminating complex `if/else` or `switch` statements. It uses a relatable smartphone notification modes analogy to explain the pattern's core components: Context, State Interface, and Concrete State Classes. The author provides a practical implementation example in Dart, demonstrating how state-specific logic is encapsulated in separate classes for cleaner and more maintainable code. The piece also discusses the advantages, such as adherence to the Open/Closed Principle, and potential drawbacks like increased class count for simple scenarios. It serves as a guide for when and when not to apply this powerful design pattern.]
---
```

# State Design Pattern in Dart: Behavioral Pattern for Managing Dynamic Object Behavior | Medium

# Stop Writing If-Else Trees: Use the State Pattern Instead

Maxim Gorin
13 min read · Apr 10, 2025

The **State design pattern** is a behavioral software pattern that allows an object to alter its behavior when its internal state changes. In simpler terms, the State pattern lets an object behave differently based on its current state, without cluttering the code with endless `if/else` or `switch` statements.

In our previous article [**Why the Command Pattern Is More Useful Than You Think**](/why-the-command-pattern-is-more-useful-than-you-think-774eb7ddb685), we explored how encapsulating actions as objects can make code more flexible. The State pattern takes a similar approach but focuses on encapsulating **states** and their behaviors as objects. Just like the Command pattern, the State pattern helps us eliminate long conditional logic and follow solid design principles — but it addresses a different kind of problem.

![Diagram illustrating the State Design Pattern concept with a central circle labeled "State Design Pattern" and radiating lines to various states or behaviors, suggesting dynamic behavior based on state.](https://miro.medium.com/v2/resize:fit:700/1*vF0QEUbReZDxGevlHfGimg.png)

# A Real-World Analogy: Phone Notification Modes

Imagine you have a smartphone with multiple notification **modes**: **Normal**, **Vibrate**, and **Silent**. In Normal mode, incoming calls ring out loud. In Vibrate mode, the phone doesn’t ring but buzzes. In Silent mode, it neither rings nor vibrates — perhaps it just logs a missed call. You (the phone’s owner) might manually switch between these modes depending on context (at work, in a meeting, at the movies, etc.), and the phone’s behavior changes accordingly without you fiddling with the internals of how ringing works each time.

This scenario is a relatable analogy for the State pattern:

*   The **Phone** is the object whose behavior changes.
*   The current **mode** (Normal/Vibrate/Silent) is the internal **state** of the phone.
*   Each state defines how the phone should behave for certain actions (like receiving a call).
*   Switching modes is like changing the internal state object, which in turn changes the phone’s behavior.

**Why not just use an if-else or enum?** You could code the phone’s behavior with a simple `if` or `switch`:

```
if (mode == NORMAL) {  
  ring loud  
} else if (mode == VIBRATE) {  
  buzz  
} else if (mode == SILENT) {  
  stay quiet  
}
```

This works fine for a few modes. But imagine if the phone had a dozen modes, each affecting multiple behaviors (calls, messages, alarms, notifications). The number of conditional branches would grow, and the logic for each mode would spread across many `if` statements throughout the code. It would become error-prone to maintain.

The State pattern provides a cleaner approach: treat each mode as a separate **State class** with its own logic. The phone will hold a reference to a state object (e.g., an instance of a `SilentState` or `VibrateState` class) and delegate behavior to it. When you change the mode, you actually swap out the state object for a different one. This way, you **avoid big conditional statements** and instead rely on polymorphism – each state class knows how to handle actions appropriately for that state.

# How the State Pattern Works

The State pattern has a few key pieces working together:

*   **Context**: The main object that has a dynamic internal state. In our analogy, the `Phone` is the context.
*   **State Interface (or Abstract Class)**: Defines the common interface for different state behaviors. It declares methods that correspond to actions the context wants to delegate. For example, a `PhoneState` interface might declare a method like `handleIncomingCall()`.
*   **Concrete State Classes**: These are the objects for states. Each concrete state class represents a specific state and implements the state interface, providing the behavior for the context in that state. E.g., `NormalState`, `VibrateState`, `SilentState` classes each implement how to handle an incoming call differently.
*   **State Transitions**: The context typically has a method to change its current state. This can happen via external triggers (e.g., user changes mode) or internal logic (the state object might decide to transition to a different state).

![UML Class Diagram for the State Design Pattern, showing a 'Context' class with an aggregation relationship to an abstract 'State' interface/class, which is then implemented by 'ConcreteStateA' and 'ConcreteStateB' classes. It illustrates how the Context delegates requests to its current State object.](https://miro.medium.com/v2/resize:fit:700/0*8ZMHnWcsxpEcMO8D.png)

When the context receives a request (like `phone.receiveCall()`), it doesn't handle it directly. Instead, it delegates the work to its current state object (e.g., `currentState.handleIncomingCall()`). Because each state object implements that action differently, the outcome varies depending on the state. In code, this is polymorphism at work: one method call results in different behavior based on the actual state object in use.

## Avoiding Conditional Complexity

A primary motivation for using the State pattern is to eliminate repetitive conditional logic scattered across the code. If an object’s behavior varies by state, you might be tempted to use an enum or flags to track state and then use `switch`/`if` statements inside every method that needs to behave differently. This leads to **bulky, hard-to-maintain code**. The State pattern solves this by localizing state-specific logic into separate classes:

*   Each state’s logic lives in its own class (e.g., all the logic for “Silent mode” is inside `SilentState`).
*   The context code becomes simpler; it no longer needs large conditional blocks for state-specific behavior.
*   Adding a new state or modifying an existing one doesn’t require editing a giant `switch` statement in multiple places – you just create a new state class or update one.

According to the classic definition, _“operations have large, multipart conditional statements that depend on the object’s state… The State pattern puts each branch of the conditional in a separate class, treating the state as an object in its own right”_. This encapsulation makes our code adhere to the Open/Closed Principle: we can introduce new states without changing the context or other states’ code. It also aligns with Single Responsibility Principle, since each state class handles one variant of behavior.

## When (and When Not) to Use State Pattern

**Use the State pattern when:**

*   An object’s behavior depends on its **current state** and it needs to change behavior at runtime depending on that state. If you find yourself writing code like “if state is X do this, if state is Y do that” in multiple places, it’s a sign that state pattern might help.
*   You have **multiple behaviors** associated with an object that could be cleanly separated. For example, the phone’s ringing behavior, vibration behavior, silent logging, etc., are distinct.
*   You want to **avoid duplication** of state-checking logic. Instead of copy-pasting the same switch on state in many methods, state pattern centralizes the behavior in the state classes.
*   You anticipate that new states might be added in the future or the logic per state will get more complex. The pattern makes it easier to extend (just add a new state class) and modify (just change one class’ code).

**When not to use (or caution):**

*   If an object has only a couple of states and very simple differences in behavior, using the State pattern can be overkill. A straightforward conditional might be more readable in such trivial cases.
*   If state changes are rare or the logic is unlikely to grow, the overhead of extra classes might not pay off.
*   If you have a fixed number of states that never change and the logic per state is straightforward, a simple enum and switch might be perfectly fine. The pattern truly shines in **complex scenarios** where states and behaviors multiply or change over time.

Think of it this way: a small state machine with two states isn’t hard to maintain with an `if`. But a state machine with ten states and complex transitions is much easier to manage with the State pattern structure.

# Why State Pattern Over Enums and Flags?

It’s common to start with an `enum` or a set of boolean flags to represent state. For instance, you might have:

```
enum Mode { normal, vibrate, silent }
```

And then write logic like:

```
if (mode == Mode.normal) {  
  // loud ring  
} else if (mode == Mode.vibrate) {  
  // vibrate  
} else if (mode == Mode.silent) {  
  // stay quiet  
}
```

This approach works, but as the software grows, it can lead to problems:

*   **Scattered Logic:** If multiple behaviors depend on the mode, you’ll have similar `if/else` or `switch` blocks in many methods (`handleCall()`, `notifyMessage()`, `alarmRing()`, etc.). Any change in a mode’s behavior means finding and updating every conditional.
*   **Violation of Open/Closed Principle:** To add a new mode (say “Do Not Disturb”), you must modify all those `switch` statements. Each modification risks introducing bugs and affects existing code.
*   **Difficult Maintenance:** The more states and conditions, the harder it is to read and maintain that code. It becomes a giant **state machine** interwoven with business logic.

The State pattern addresses these issues by **encapsulating state-specific behaviors**. Instead of one big function with many branches, you have many small classes each handling one branch. This leads to cleaner separation:

*   The **Phone** class (context) no longer needs to know details of each mode’s behavior. It simply delegates to the current state object.
*   Adding **Do Not Disturb** mode, for example, means making a new `DoNotDisturbState` class implementing the desired behaviors. The Phone class might only need a minor change (or even none, if the state can be set via a setter or some factory).
*   Removing or changing a state’s behavior affects only that state class, reducing risk to other parts of the code.

**In short:** In complex scenarios, the State pattern provides a more robust, flexible approach than enums/flags with conditionals. It keeps code modular and adheres to design principles, making it easier for multiple developers (frontend, backend, mobile, etc.) to follow the logic without sifting through tangled conditions.

# Pros and Cons of the State Pattern

Like any design pattern, State has its benefits and trade-offs. Let’s break them down:

**Pros:**

*   **Cleaner Code Organization:** State-specific code is isolated in separate classes. This satisfies Single Responsibility Principle, since each state class focuses on one set of behaviors (one “mode” of the object).
*   **Eliminates Complex Conditionals:** The context code is freed from lengthy `if/else` chains or switch statements for different states. This often means the context class (like our `Phone`) becomes simpler and easier to maintain.
*   **Open/Closed Principle Friendly:** You can add new states without modifying existing ones or the context, which aligns with the Open/Closed Principle. For instance, adding a new phone mode doesn’t require touching the logic for other modes.
*   **State Transition Encapsulation:** The logic for transitioning from one state to another can be controlled in one place. Depending on your design, either the context or the state objects handle transitions, making the flow of states easier to manage and understand.
*   **Polymorphic Behavior:** The pattern leverages polymorphism. You can introduce new behavior by just swapping out the state object at runtime. The rest of the system can remain oblivious to the change, which can reduce bugs — the context just calls a method that happens to do something different now.

**Cons:**

*   **More Classes and Complexity:** Introducing the State pattern means creating multiple classes (one for each state). For simple situations, this can feel like over-engineering. The overhead of understanding the extra indirection might not be worth it if a simple conditional would do.
*   **State Explosion:** If an object can have **many** states, you’ll have lots of classes. Managing transitions between a large number of states can become complex in its own right. (Mitigation: group related states or use hierarchical state patterns, or consider if all those states are truly needed.)
*   **Tight Coupling of States and Context:** State objects often need to know about their context (to change state or query context data) and sometimes about other states (if one state decides to switch to another). This can introduce coupling between state classes. However, this is usually controlled and localized coupling. It’s often an acceptable trade-off for eliminating global complexity, but it’s something to be aware of.
*   **Learning Curve:** For some developers (especially those not familiar with design patterns), the indirection of “an object having an object to do its work” can be confusing at first. It might be less straightforward than a quick `if` check when reading code until you get used to the pattern.
*   **Memory/Performance Overhead:** In some languages, creating objects for states might have a slight performance cost (though in most cases this is negligible). If state objects hold a lot of data duplicated from the context, that could be inefficient. In practice, state objects are often lightweight or even singletons, so this is rarely a big issue.

**Mitigating the Downsides:** If you have concerns about too many classes, you can sometimes implement state objects as inner classes or even anonymous classes (in languages that support it) to keep them grouped with the context. If you worry about object creation overhead, you can reuse state instances (the state pattern doesn’t mandate new object every time; you can keep singletons or stateless instances). Also, good naming and documentation can help the learning curve by making the role of each state class clear.

# Implementing the State Pattern in Dart (Phone Mode Example)

To solidify the concepts, let’s implement our smartphone **notification mode** example in Dart. We will create a simple simulation of a phone receiving calls in different modes. The code will be self-contained and print output to the console (so you can run it in an online Dart playground or similar).

**Design of the example:**

*   We have an abstract `PhoneState` class that defines what happens when the phone receives a call (`onReceiveCall` method).
*   We have concrete state classes: `NormalState`, `VibrateState`, and `SilentState` that extend `PhoneState` and implement `onReceiveCall` differently.
*   The `Phone` class is our context. It has a `state` property of type `PhoneState`. It delegates the `receiveCall()` action to the current state. It also provides a method to change the mode/state (`setState()`).
*   We’ll simulate the phone in different modes receiving calls to see how it behaves.

Here’s the Dart code:

```dart
// State interface (abstract class in Dart)  
abstract class PhoneState {  
  void onReceiveCall(Phone context);  
}

// Concrete State: Normal mode (ring loudly)  
class NormalState implements PhoneState {  
  @override  
  void onReceiveCall(Phone context) {    
    print("Incoming call: Ring ring! 📢 (Normal mode)");    
    // In normal mode, the phone rings loudly.    
    // (No state change occurs automatically in this mode.)  
  }  
}

// Concrete State: Vibrate mode  
class VibrateState implements PhoneState {  
  int _vibrateCount = 0;  // example of internal state (e.g., count calls)  
  @override  
  void onReceiveCall(Phone context) {  
    _vibrateCount++;    
    print("Incoming call: Bzzzt bzzzt... 🤫 (Vibrate mode)");    
    // Perhaps if too many calls come in vibrate, switch to silent automatically:    
    if (_vibrateCount >= 3) {      
      print("No answer after $_vibrateCount vibrations, switching to Silent mode.");  
      context.setState(SilentState());      
      // Note: This is just to demonstrate a state triggering a transition.      
      // In real life, phones don't usually do this on their own!    
    }  
  }  
}

// Concrete State: Silent mode  
class SilentState implements PhoneState {  
  @override  
  void onReceiveCall(Phone context) {    
    print("Incoming call: (Silent mode, no sound) 🤐");    
    print("The phone stays silent. You might see a missed call later.");    
    // In silent mode, maybe log a missed call in context (omitted for simplicity).  
  }  
}

// Context: Phone  
class Phone {  
  // start in Normal mode by default  
  PhoneState _state = NormalState();  
  void setState(PhoneState newState) {  
    _state = newState;    
    // You could also print or log the mode change here if desired.  
  }  
  void receiveCall() {    
    // Delegate behavior to the current state object    
    _state.onReceiveCall(this);  
  }  
  // (Optional) A helper to show current state as a string, for logging:  
  String get modeName => _state.runtimeType.toString();  
}

void main() {  
  Phone phone = Phone();  
  print("Phone is now in ${phone.modeName}.");  
  // Simulate incoming calls in Normal mode  
  phone.receiveCall();  // should ring loudly  
  // Change to Vibrate mode  
  phone.setState(VibrateState());  
  print("\nPhone is now in ${phone.modeName}.");  
  
  phone.receiveCall();  // vibrate  
  phone.receiveCall();  // vibrate again  
  phone.receiveCall();  // vibrate third time, triggers switch to Silent  
  // Now phone should have switched to Silent mode automatically  
  print("\nPhone is now in ${phone.modeName}.");  
  
  phone.receiveCall();  // silent, no sound  
  // Manually switch back to Normal mode  
  phone.setState(NormalState());  
  print("\nPhone is now in ${phone.modeName}.");  
  
  phone.receiveCall();  // rings loudly again  
}
```

In the code above, note a few important things:

*   The `Phone` context doesn’t know what happens when a call