# Reactive Services

Reactive Services is a framework that support the development of distributed applications over the [.NET/Mono](http://www.mono-project.com/) frameworks, following the principles of the [Reactive Manifesto](http://www.reactivemanifesto.org) and ready for cloud distribution.

See [this link](http://reactiveservices.github.io) for an overview about the framework.

[![Build status](https://ci.appveyor.com/api/projects/status/0jqpqlrydwh2s7xb?svg=true)](https://ci.appveyor.com/project/rafaelromao/reactiveservices-application)

## ReactiveServices.Application

ReactiveServices.Application is the main package of the Reactive Services framework.

### Reactive Services Components

This package provides the following components:

- **ReactiveServices.Application.Bootstrap**: This class is used to bootstrap your application. Calling its static `Run()` method will load the `Bootstrap.config` file and initialize the computational units according to the configuration found in this file. A `Dependencies.config` file must also exist and be used to inform the system how to resolve its dependencies, as well as a `Settings.config`file, to give to the system any custom parameterization, such as connections string, for example. A `NLog.config` file shall also be present and will be used to instrument the NLog framework about the parameterization it shall use.
- **ReactiveServices.Application.Coordinator**: This class is used by the `Boostrap` class to instanciate a `Supervisor` for your computational units and continue with the bootstrap process.
- **ReactiveServices.Application.Supervisor**: This class is responsible for supervising the computational units of a given `Bootstrap.config` file and restore their execution if they seem to be down. This process is also used for the initial bootstrap of the application.
- **ReactiveServices.Application.Bootstrapper**: This is the class that will perform the actual bootstrap of a given `Boostrap.config` file. It is used by the `Supervisor` class also to bootstrap individual computational units.
- **ReactiveServices.Application.DispatcherLauncher**: This class is used by the `Bootstrapper` class to perform the launch of a single computational unit. An `InProcessDispatcherLauncher` version is also available and can be configured using the `Dependencies.config` file and will allow the bootstrap of the entire set of `Dispatchers` inside the process that makes the call, without launching any `ComputationalUnit.exe` process. In this case, the calling process will act as a single computational unit for all configured workers.
- **ReactiveServices.ComputationalUnit.Dispatching.Dispatcher**: This class is the main service executed inside a `ComputationalUnit.exe` process. It is responsible for receiving the job requests from the message bus, instanciate the corresponding workers, according to the configuration in the `Bootstrap.config` file and dispatch the job execution to such workers, as well as monitor their lifecycle.
- **ComputationalUnit.exe**: This program will host the set (or a subset) of workers that will compose your application. Multiple instances of this program can be configured independently to allow your application to scale according to your needs.
- **ReactiveServices.ComputationalUnit.Work.Worker**: This class must be used as base class for any worker and instrument the developer with the abstract methods that must be implemented to allow the full lifecycle of the worker to be completed.
- **ReactiveServices.ComputationalUnit.Work.Job**: This class must be used as base class for the job requests that will be published in the message bus and executed by the corresponding worker.

### How it works?

#### Publishing a job request

A job request is any class derived from the `ReactiveServices.ComputationalUnit.Work.Job` class. It must be annotated with the `[DataContract]` and `[DataMember]` attributes and must be published on the message bus in an exchange named `JobClassFullName:JobClassAssembly` with the routing key `pending`. This publication must be performed after the computational unit that hosts the corresponding worker be running.

#### Executing a job

Once published, the job request will be received by the `Dispatcher` and dispatched to the corresponding worker. This job/worker correspondence must be configured in the `Bootstrap.config` file.
The worker which the job request was dispatched to will execute the job and publish some events upon its conclusion. In case of a failure, the `Dispatcher` may try to repeat the execution, according to the configuration. In case the whole computational unit crashes, the job request will return to the message bus as a pending request and can be received by another computational unit configured to host the same kind of worker.

#### Talking to the Message Bus

Any process of publish or receive a message from the Message Bus must be performed using the **ReactiveServices.MessageBus** package. See the [package documentation](https://github.com/ReactiveServices/ReactiveServices.MessageBus) for more details.

## License

MIT License

## Versioning

SemVer 2.0