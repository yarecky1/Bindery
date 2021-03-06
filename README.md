Bindery
=======
Bindery aims to support fluent MVVM binding definition for WinForms applications.

Projects
--------
* **Bindery:** 
  * Contains the static `Create` factory class 
  * Dependent on the [System.Reactive](https://www.nuget.org/packages/System.Reactive/) package
  * Please note that it may be necessary to install the [System.Reactive.Compatibility](https://www.nuget.org/packages/System.Reactive.Compatibility/) package in the main project of 
    a consuming application in order to resolve conflicts between the [System.Reactive](https://www.nuget.org/packages/System.Reactive/) package 
    and older versions of `System.Reactive`-related libraries (such as `Sytem.Reactive.Linq`).

Assumptions
-----------
* A **View Model** is a binding source, an object of any type. Full binding functionality requires a View Model to properly implement
 [INotifyPropertyChanged](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged).
* A **Control** is a binding target, an object that implements [IBindableComponent](https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.ibindablecomponent). 
A Control supports the full range of binding functionality.
* A **Target** is a binding target, an object of any type. A Target only supports a limited set of binding functionality.
* A **Command** is an object that implements [ICommand](https://docs.microsoft.com/en-us/dotnet/api/system.windows.input.icommand).

Code Examples
-------------
### Binding
##### Create a root binder for the view model
```C#
var binder = Bindery.Create.Binder(viewModel);
```
##### Create a root binder and set the default subscription scheduler to schedule actions on the form's thread
```C#
var binder = Bindery.Create.Binder(viewModel, new ControlScheduler(form));
```
##### Dispose of a binder
Diposing of a binder removes all bindings and disposes of all subcriptions created by the binder.
```C#
binder.Dispose();
```
##### Register external disposables with the binder
This can be used to tie the lifetime of other objects to the binder's lifetime.
```C#
binder.RegisterDisposable(disposableViewModel, disposableCommand);
```
##### Bind a view model property to a control property
```C#
// Two-way binding
binder.Control(textBox).Property(c => c.Text).Bind(vm => vm.Name); 
// One-way binding from source to control
binder.Control(form).Property(c => c.UseWaitCursor).Get(vm => vm.IsBusy); 
// One-way binding from control to source
binder.Control(textBox).Property(c => c.Text).Set(vm => vm.Name); 
```
##### Bind an integer view model property to a string control property
```C#
binder.Control(textBox).Property(c => c.Text).Get(vm => vm.Age, Convert.ToString);
```
##### Bind a button's `Click` event to a command
This also "binds" the control's `Enabled` property to the command's `CanExecute` method.
```C#
ICommand command = new CommandImplementation(viewModel);
binder.Control(button).OnClick(command);
```
##### Bind a form's `MouseMove` event to a command
```C#
ICommand command = new CommandImplementation(viewModel);
binder.Control(form).OnEvent<MouseEventArgs>("MouseMove")
  .Transform(o => o.Where(ctx => ctx.Args.Button==MouseButtons.Left).Select(ctx => new {ctx.Args.X, ctx.Args.Y})) 
  // Mouse coords are passed to command.Execute()
  .Execute(command);
```
##### Bind a form's `MouseMove` event arguments to a view model property
```C#
binder.Control(form).OnEvent<MouseEventArgs>("MouseMove")
  .Transform(o => o.Select(ctx => new MyCoord{X = ctx.Args.X, Y = ctx.Args.Y}))
  .Set(vm => vm.CurrentMouseCoords);
```
##### Bind to a non-control target object
Non-control targets support a limited set of binding options. Two-way binding and one-way binding from target to source are not supported.
```C#
binder.Target(target).Property(t => t.Status).Get(vm => vm.Status);
```
### Observable subscriptions

##### Trigger an action when a view model property changes
```C#
binder.OnPropertyChanged(vm => vm.ErrorMessage).Subscribe(msg => DisplayErrorDialog(msg));
```
##### Subscribe to a button's `Click` event to close the form
```C#
binder.Control(cancelButton).OnClick().Subscribe(ctx => form.Close());
```
##### Subscribe to a form's `Closed` event to dispose of the binder
```C#
binder.Control(form).OnEvent("Closed").Subscribe(ctx => binder.Dispose());
```
##### Create an observable subscription to execute a command
```C#
binder.Observe(viewModel.Observable).Execute(command);
```
##### Overriding the default scheduler to execute the command immediately on each observed object
```C#
binder.Observe(viewModel.Observable).ObserveOn(Scheduler.Immediate).Execute(command);
```
##### Subscribe to an observable with full subscription syntax support
```C#
binder.Observe(viewModel.Observable).Subscribe(
  ctx=>ctx.OnNext(oVal => OnNextAction(oVal))
       .OnError(ex => HandleException(ex))
       .OnComplete(() => OnCompleteAction()));
```
##### Create an observable subscription to call an async method
```C#
binder.Observe(viewModel.Observable).SubscribeAsync(msg => command.ExecuteAsync(msg.Value));
```
### Event to observable conversion
```C#
IObservable<string> mouseMoveButtons =
  Bindery.Create.ObservableFor(form).Event<MouseEventArgs>("MouseMove")
       .Select(ctx => Convert.ToString(ctx.Args.Button));
```
