using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Controls;
using Caliburn.Micro;
using Ninject.Extensions.Conventions;
using Ninject;
using TimeRecorder.Main;

namespace TimeRecorder
{
    public class NInjectBootstrapper : Bootstrapper<IMainViewModel>
    {
        private IKernel _kernel;

        protected override void Configure()
        {
            _kernel = new StandardKernel();

            _kernel.Bind<IWindowManager>()
                .To<WindowManager>().InSingletonScope();

            _kernel.Bind<IEventAggregator>()
                .To<EventAggregator>().InSingletonScope();

            _kernel.Bind<IMainViewModel>()
                .To<MainViewModel>().InSingletonScope();

            // scan services
            _kernel.Bind(scanner => scanner
                .FromAssemblyContaining<IMainViewModel>()
                .Select(IsServiceType)
                .BindDefaultInterface()
                .Configure(binding => binding.InSingletonScope()));

            // scan view models
            _kernel.Bind(scanner => scanner
                .FromAssemblyContaining<IMainViewModel>()
                .Select(IsViewModelType)
                .BindDefaultInterface());

            ConventionManager.AddElementConvention<DatePicker>(
                DatePicker.SelectedDateProperty, "SelectedDate", "Loaded");
        }

        private static bool IsViewModelType(Type type)
        {
            return !type.Name.EndsWith("Service") &&
                   !type.Name.EndsWith("MainViewModel");
        }

        private static bool IsServiceType(Type type)
        {
            return type.Name.EndsWith("Service");
        }

        protected override object GetInstance(Type service, string key)
        {
            return _kernel.Get(service);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _kernel.GetAll(service);
        }

        protected override void BuildUp(object instance)
        {
            _kernel.Inject(instance);
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            return new[]
            {
                Assembly.GetExecutingAssembly(),    // this is Shell assembly
            };
        }
    }
}