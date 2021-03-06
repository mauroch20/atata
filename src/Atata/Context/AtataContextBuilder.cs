﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Opera;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Safari;

namespace Atata
{
    /// <summary>
    /// Represents the builder of <see cref="AtataContext"/>.
    /// </summary>
    public class AtataContextBuilder
    {
        public AtataContextBuilder(AtataBuildingContext buildingContext)
        {
            BuildingContext = buildingContext.CheckNotNull(nameof(buildingContext));
        }

        /// <summary>
        /// Gets the building context.
        /// </summary>
        public AtataBuildingContext BuildingContext { get; internal set; }

        /// <summary>
        /// Gets the builder of context attributes,
        /// which provides the functionality to add extra attributes to different metadata levels:
        /// global, assembly, component and property.
        /// </summary>
        public AttributesAtataContextBuilder Attributes => new AttributesAtataContextBuilder(BuildingContext);

        /// <summary>
        /// Use the driver factory.
        /// </summary>
        /// <typeparam name="TDriverFactory">The type of the driver factory.</typeparam>
        /// <param name="driverFactory">The driver factory.</param>
        /// <returns>The <typeparamref name="TDriverFactory"/> instance.</returns>
        public TDriverFactory UseDriver<TDriverFactory>(TDriverFactory driverFactory)
            where TDriverFactory : AtataContextBuilder, IDriverFactory
        {
            driverFactory.CheckNotNull(nameof(driverFactory));

            BuildingContext.DriverFactories.Add(driverFactory);
            BuildingContext.DriverFactoryToUse = driverFactory;

            return driverFactory;
        }

        /// <summary>
        /// Sets the alias of the driver to use.
        /// </summary>
        /// <param name="alias">The alias of the driver.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseDriver(string alias)
        {
            alias.CheckNotNullOrWhitespace(nameof(alias));

            IDriverFactory driverFactory = BuildingContext.DriverFactories.LastOrDefault(x => alias.Equals(x.Alias, StringComparison.CurrentCultureIgnoreCase));

            if (driverFactory != null)
                BuildingContext.DriverFactoryToUse = driverFactory;
            else if (UsePredefinedDriver(alias) == null)
                throw new ArgumentException($"No driver with \"{alias}\" alias defined.", nameof(alias));

            return this;
        }

        /// <summary>
        /// Use specified driver instance.
        /// </summary>
        /// <param name="driver">The driver to use.</param>
        /// <returns>The <see cref="CustomDriverAtataContextBuilder"/> instance.</returns>
        public CustomDriverAtataContextBuilder UseDriver(RemoteWebDriver driver)
        {
            driver.CheckNotNull(nameof(driver));

            return UseDriver(() => driver);
        }

        /// <summary>
        /// Use custom driver factory method.
        /// </summary>
        /// <param name="driverFactory">The driver factory method.</param>
        /// <returns>The <see cref="CustomDriverAtataContextBuilder"/> instance.</returns>
        public CustomDriverAtataContextBuilder UseDriver(Func<RemoteWebDriver> driverFactory)
        {
            driverFactory.CheckNotNull(nameof(driverFactory));

            return UseDriver(new CustomDriverAtataContextBuilder(BuildingContext, driverFactory));
        }

        private IDriverFactory UsePredefinedDriver(string alias)
        {
            switch (alias.ToLowerInvariant())
            {
                case DriverAliases.Chrome:
                    return UseChrome();
                case DriverAliases.Firefox:
                    return UseFirefox();
                case DriverAliases.InternetExplorer:
                    return UseInternetExplorer();
                case DriverAliases.Safari:
                    return UseSafari();
                case DriverAliases.Opera:
                    return UseOpera();
                case DriverAliases.Edge:
                    return UseEdge();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Use the <see cref="ChromeDriver"/>.
        /// </summary>
        /// <returns>The <see cref="ChromeAtataContextBuilder"/> instance.</returns>
        public ChromeAtataContextBuilder UseChrome()
        {
            return UseDriver(new ChromeAtataContextBuilder(BuildingContext));
        }

        /// <summary>
        /// Use the <see cref="FirefoxDriver"/>.
        /// </summary>
        /// <returns>The <see cref="FirefoxAtataContextBuilder"/> instance.</returns>
        public FirefoxAtataContextBuilder UseFirefox()
        {
            return UseDriver(new FirefoxAtataContextBuilder(BuildingContext));
        }

        /// <summary>
        /// Use the <see cref="InternetExplorerDriver"/>.
        /// </summary>
        /// <returns>The <see cref="InternetExplorerAtataContextBuilder"/> instance.</returns>
        public InternetExplorerAtataContextBuilder UseInternetExplorer()
        {
            return UseDriver(new InternetExplorerAtataContextBuilder(BuildingContext));
        }

        /// <summary>
        /// Use the <see cref="EdgeDriver"/>.
        /// </summary>
        /// <returns>The <see cref="EdgeAtataContextBuilder"/> instance.</returns>
        public EdgeAtataContextBuilder UseEdge()
        {
            return UseDriver(new EdgeAtataContextBuilder(BuildingContext));
        }

        /// <summary>
        /// Use the <see cref="OperaDriver"/>.
        /// </summary>
        /// <returns>The <see cref="OperaAtataContextBuilder"/> instance.</returns>
        public OperaAtataContextBuilder UseOpera()
        {
            return UseDriver(new OperaAtataContextBuilder(BuildingContext));
        }

        /// <summary>
        /// Use the <see cref="SafariDriver"/>.
        /// </summary>
        /// <returns>The <see cref="SafariAtataContextBuilder"/> instance.</returns>
        public SafariAtataContextBuilder UseSafari()
        {
            return UseDriver(new SafariAtataContextBuilder(BuildingContext));
        }

        /// <summary>
        /// Use the <see cref="RemoteWebDriver"/>.
        /// </summary>
        /// <returns>The <see cref="RemoteDriverAtataContextBuilder"/> instance.</returns>
        public RemoteDriverAtataContextBuilder UseRemoteDriver()
        {
            return UseDriver(new RemoteDriverAtataContextBuilder(BuildingContext));
        }

        /// <summary>
        /// Adds the log consumer.
        /// </summary>
        /// <typeparam name="TLogConsumer">
        /// The type of the log consumer.
        /// Should have default constructor.
        /// </typeparam>
        /// <returns>The <see cref="AtataContextBuilder{TLogConsumer}"/> instance.</returns>
        public AtataContextBuilder<TLogConsumer> AddLogConsumer<TLogConsumer>()
            where TLogConsumer : ILogConsumer, new() =>
            AddLogConsumer(new TLogConsumer());

        /// <summary>
        /// Adds the log consumer.
        /// </summary>
        /// <typeparam name="TLogConsumer">The type of the log consumer.</typeparam>
        /// <param name="consumer">The log consumer.</param>
        /// <returns>The <see cref="AtataContextBuilder{TLogConsumer}"/> instance.</returns>
        public AtataContextBuilder<TLogConsumer> AddLogConsumer<TLogConsumer>(TLogConsumer consumer)
            where TLogConsumer : ILogConsumer
        {
            consumer.CheckNotNull(nameof(consumer));

            BuildingContext.LogConsumers.Add(new LogConsumerInfo(consumer));
            return new AtataContextBuilder<TLogConsumer>(consumer, BuildingContext);
        }

        /// <summary>
        /// Adds the log consumer.
        /// </summary>
        /// <param name="typeNameOrAlias">The type name or alias of the log consumer.</param>
        /// <returns>The <see cref="AtataContextBuilder{TLogConsumer}"/> instance.</returns>
        public AtataContextBuilder<ILogConsumer> AddLogConsumer(string typeNameOrAlias)
        {
            ILogConsumer consumer = LogConsumerAliases.Resolve(typeNameOrAlias);
            return AddLogConsumer(consumer);
        }

        /// <summary>
        /// Adds the <see cref="TraceLogConsumer"/> instance that uses <see cref="Trace"/> class for logging.
        /// </summary>
        /// <returns>The <see cref="AtataContextBuilder{TraceLogConsumer}"/> instance.</returns>
        public AtataContextBuilder<TraceLogConsumer> AddTraceLogging()
        {
            return AddLogConsumer(new TraceLogConsumer());
        }

        /// <summary>
        /// Adds the <see cref="DebugLogConsumer"/> instance that uses <see cref="Debug"/> class for logging.
        /// </summary>
        /// <returns>The <see cref="AtataContextBuilder{DebugLogConsumer}"/> instance.</returns>
        public AtataContextBuilder<DebugLogConsumer> AddDebugLogging()
        {
            return AddLogConsumer(new DebugLogConsumer());
        }

        /// <summary>
        /// Adds the <see cref="ConsoleLogConsumer"/> instance that uses <see cref="Console"/> class for logging.
        /// </summary>
        /// <returns>The <see cref="AtataContextBuilder{ConsoleLogConsumer}"/> instance.</returns>
        public AtataContextBuilder<ConsoleLogConsumer> AddConsoleLogging()
        {
            return AddLogConsumer(new ConsoleLogConsumer());
        }

        /// <summary>
        /// Adds the <see cref="NUnitTestContextLogConsumer"/> instance that uses <c>NUnit.Framework.TestContext</c> class for logging.
        /// </summary>
        /// <returns>The <see cref="AtataContextBuilder{NUnitTestContextLogConsumer}"/> instance.</returns>
        public AtataContextBuilder<NUnitTestContextLogConsumer> AddNUnitTestContextLogging()
        {
            return AddLogConsumer(new NUnitTestContextLogConsumer());
        }

        /// <summary>
        /// Adds the <see cref="NLogConsumer"/> instance that uses <c>NLog.Logger</c> class for logging.
        /// </summary>
        /// <param name="loggerName">The name of the logger.</param>
        /// <returns>The <see cref="AtataContextBuilder{NLogConsumer}"/> instance.</returns>
        public AtataContextBuilder<NLogConsumer> AddNLogLogging(string loggerName = null)
        {
            return AddLogConsumer(new NLogConsumer(loggerName));
        }

        /// <summary>
        /// Adds the <see cref="Log4NetConsumer"/> instance that uses <c>log4net.ILog</c> interface for logging.
        /// </summary>
        /// <param name="loggerName">The name of the logger.</param>
        /// <returns>The <see cref="AtataContextBuilder{Log4NetConsumer}"/> instance.</returns>
        public AtataContextBuilder<Log4NetConsumer> AddLog4NetLogging(string loggerName = null)
        {
            return AddLogConsumer(new Log4NetConsumer { LoggerName = loggerName });
        }

        /// <summary>
        /// Adds the <see cref="Log4NetConsumer"/> instance that uses <c>log4net.ILog</c> interface for logging.
        /// </summary>
        /// <param name="repositoryName">The name of the logger repository.</param>
        /// <param name="loggerName">The name of the logger.</param>
        /// <returns>The <see cref="AtataContextBuilder{Log4NetConsumer}"/> instance.</returns>
        public AtataContextBuilder<Log4NetConsumer> AddLog4NetLogging(string repositoryName, string loggerName = null)
        {
            return AddLogConsumer(new Log4NetConsumer { RepositoryName = repositoryName, LoggerName = loggerName });
        }

        /// <summary>
        /// Adds the <see cref="Log4NetConsumer"/> instance that uses <c>log4net.ILog</c> interface for logging.
        /// </summary>
        /// <param name="repositoryAssembly">The assembly to use to lookup the repository.</param>
        /// <param name="loggerName">The name of the logger.</param>
        /// <returns>The <see cref="AtataContextBuilder{Log4NetConsumer}"/> instance.</returns>
        public AtataContextBuilder<Log4NetConsumer> AddLog4NetLogging(Assembly repositoryAssembly, string loggerName = null)
        {
            return AddLogConsumer(new Log4NetConsumer { RepositoryAssembly = repositoryAssembly, LoggerName = loggerName });
        }

        /// <summary>
        /// Adds the screenshot consumer.
        /// </summary>
        /// <typeparam name="TScreenshotConsumer">The type of the screenshot consumer.</typeparam>
        /// <returns>The <see cref="AtataContextBuilder{TLogConsumer}"/> instance.</returns>
        public AtataContextBuilder<TScreenshotConsumer> AddScreenshotConsumer<TScreenshotConsumer>()
            where TScreenshotConsumer : IScreenshotConsumer, new() =>
            AddScreenshotConsumer(new TScreenshotConsumer());

        /// <summary>
        /// Adds the screenshot consumer.
        /// </summary>
        /// <typeparam name="TScreenshotConsumer">The type of the screenshot consumer.</typeparam>
        /// <param name="consumer">The screenshot consumer.</param>
        /// <returns>The <see cref="AtataContextBuilder{TLogConsumer}"/> instance.</returns>
        public AtataContextBuilder<TScreenshotConsumer> AddScreenshotConsumer<TScreenshotConsumer>(TScreenshotConsumer consumer)
            where TScreenshotConsumer : IScreenshotConsumer
        {
            consumer.CheckNotNull(nameof(consumer));

            BuildingContext.ScreenshotConsumers.Add(consumer);
            return new AtataContextBuilder<TScreenshotConsumer>(consumer, BuildingContext);
        }

        /// <summary>
        /// Adds the screenshot consumer.
        /// </summary>
        /// <param name="typeNameOrAlias">The type name or alias of the log consumer.</param>
        /// <returns>The <see cref="AtataContextBuilder{TLogConsumer}"/> instance.</returns>
        public AtataContextBuilder<IScreenshotConsumer> AddScreenshotConsumer(string typeNameOrAlias)
        {
            IScreenshotConsumer consumer = ScreenshotConsumerAliases.Resolve(typeNameOrAlias);

            return AddScreenshotConsumer(consumer);
        }

        /// <summary>
        /// Adds the <see cref="FileScreenshotConsumer"/> instance for the screenshot saving to file.
        /// By default uses <c>"Logs\{build-start}\{test-name-sanitized}"</c> as folder path format,
        /// <c>"{screenshot-number:D2} - {screenshot-pageobjectfullname}{screenshot-title: - *}"</c> as file name format
        /// and <see cref="OpenQA.Selenium.ScreenshotImageFormat.Png"/> as image format.
        /// Example of screenshot file path using default settings: <c>"Logs\2018-03-03 14_34_04\SampleTest\01 - Home page - Screenshot title.png"</c>.
        /// Available path variables are:
        /// <c>{build-start}</c>, <c>{test-name}</c>, <c>{test-name-sanitized}</c>,
        /// <c>{test-start}</c>, <c>{driver-alias}</c>, <c>{screenshot-number}</c>,
        /// <c>{screenshot-title}</c>, <c>{screenshot-pageobjectname}</c>,
        /// <c>{screenshot-pageobjecttypename}</c>, <c>{screenshot-pageobjectfullname}</c>.
        /// Path variables support the formatting.
        /// </summary>
        /// <returns>The <see cref="AtataContextBuilder{FileScreenshotConsumer}"/> instance.</returns>
        public AtataContextBuilder<FileScreenshotConsumer> AddScreenshotFileSaving()
        {
            return AddScreenshotConsumer(new FileScreenshotConsumer());
        }

        /// <summary>
        /// Sets the factory method of the test name.
        /// </summary>
        /// <param name="testNameFactory">The factory method of the test name.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseTestName(Func<string> testNameFactory)
        {
            testNameFactory.CheckNotNull(nameof(testNameFactory));

            BuildingContext.TestNameFactory = testNameFactory;
            return this;
        }

        /// <summary>
        /// Sets the name of the test.
        /// </summary>
        /// <param name="testName">The name of the test.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseTestName(string testName)
        {
            testName.CheckNotNull(nameof(testName));

            BuildingContext.TestNameFactory = () => testName;
            return this;
        }

        /// <summary>
        /// Sets the base URL.
        /// </summary>
        /// <param name="baseUrl">The base URL.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseBaseUrl(string baseUrl)
        {
            if (baseUrl != null && !Uri.IsWellFormedUriString(baseUrl, UriKind.Absolute))
                throw new ArgumentException($"Invalid URL format \"{baseUrl}\".", nameof(baseUrl));

            BuildingContext.BaseUrl = baseUrl;
            return this;
        }

        /// <summary>
        /// Sets the base retry timeout.
        /// The default value is <c>5</c> seconds.
        /// </summary>
        /// <param name="timeout">The retry timeout.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        [Obsolete("Use UseBaseRetryTimeout instead.")] // Obsolete since v0.17.0.
        public AtataContextBuilder UseRetryTimeout(TimeSpan timeout)
        {
            return UseBaseRetryTimeout(timeout);
        }

        /// <summary>
        /// Sets the base retry interval.
        /// The default value is <c>500</c> milliseconds.
        /// </summary>
        /// <param name="interval">The retry interval.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        [Obsolete("Use UseBaseRetryInterval instead.")] // Obsolete since v0.17.0.
        public AtataContextBuilder UseRetryInterval(TimeSpan interval)
        {
            return UseBaseRetryInterval(interval);
        }

        /// <summary>
        /// Sets the base retry timeout.
        /// The default value is <c>5</c> seconds.
        /// </summary>
        /// <param name="timeout">The retry timeout.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseBaseRetryTimeout(TimeSpan timeout)
        {
            BuildingContext.BaseRetryTimeout = timeout;
            return this;
        }

        /// <summary>
        /// Sets the base retry interval.
        /// The default value is <c>500</c> milliseconds.
        /// </summary>
        /// <param name="interval">The retry interval.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseBaseRetryInterval(TimeSpan interval)
        {
            BuildingContext.BaseRetryInterval = interval;
            return this;
        }

        /// <summary>
        /// Sets the element find timeout.
        /// The default value is taken from <see cref="AtataBuildingContext.BaseRetryTimeout"/>, which is equal to <c>5</c> seconds by default.
        /// </summary>
        /// <param name="timeout">The retry timeout.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseElementFindTimeout(TimeSpan timeout)
        {
            BuildingContext.ElementFindTimeout = timeout;
            return this;
        }

        /// <summary>
        /// Sets the element find retry interval.
        /// The default value is taken from <see cref="AtataBuildingContext.BaseRetryInterval"/>, which is equal to <c>500</c> milliseconds by default.
        /// </summary>
        /// <param name="interval">The retry interval.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseElementFindRetryInterval(TimeSpan interval)
        {
            BuildingContext.ElementFindRetryInterval = interval;
            return this;
        }

        /// <summary>
        /// Sets the waiting timeout.
        /// The default value is taken from <see cref="AtataBuildingContext.BaseRetryTimeout"/>, which is equal to <c>5</c> seconds by default.
        /// </summary>
        /// <param name="timeout">The retry timeout.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseWaitingTimeout(TimeSpan timeout)
        {
            BuildingContext.WaitingTimeout = timeout;
            return this;
        }

        /// <summary>
        /// Sets the waiting retry interval.
        /// The default value is taken from <see cref="AtataBuildingContext.BaseRetryInterval"/>, which is equal to <c>500</c> milliseconds by default.
        /// </summary>
        /// <param name="interval">The retry interval.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseWaitingRetryInterval(TimeSpan interval)
        {
            BuildingContext.WaitingRetryInterval = interval;
            return this;
        }

        /// <summary>
        /// Sets the verification timeout.
        /// The default value is taken from <see cref="AtataBuildingContext.BaseRetryTimeout"/>, which is equal to <c>5</c> seconds by default.
        /// </summary>
        /// <param name="timeout">The retry timeout.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseVerificationTimeout(TimeSpan timeout)
        {
            BuildingContext.VerificationTimeout = timeout;
            return this;
        }

        /// <summary>
        /// Sets the verification retry interval.
        /// The default value is taken from <see cref="AtataBuildingContext.BaseRetryInterval"/>, which is equal to <c>500</c> milliseconds by default.
        /// </summary>
        /// <param name="interval">The retry interval.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseVerificationRetryInterval(TimeSpan interval)
        {
            BuildingContext.VerificationRetryInterval = interval;
            return this;
        }

        /// <summary>
        /// Sets the culture.
        /// The default value is <see cref="CultureInfo.CurrentCulture"/>.
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseCulture(CultureInfo culture)
        {
            BuildingContext.Culture = culture;
            return this;
        }

        /// <summary>
        /// Sets the culture by the name.
        /// The default value is <see cref="CultureInfo.CurrentCulture"/>.
        /// </summary>
        /// <param name="cultureName">The name of the culture.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseCulture(string cultureName)
        {
            return UseCulture(CultureInfo.GetCultureInfo(cultureName));
        }

        /// <summary>
        /// Sets the type of the assertion exception.
        /// The default value is a type of <see cref="AssertionException"/>.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseAssertionExceptionType<TException>()
            where TException : Exception
        {
            return UseAssertionExceptionType(typeof(TException));
        }

        /// <summary>
        /// Sets the type of the assertion exception.
        /// The default value is a type of <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="exceptionType">The type of the exception.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseAssertionExceptionType(Type exceptionType)
        {
            exceptionType.CheckIs<Exception>(nameof(exceptionType));

            BuildingContext.AssertionExceptionType = exceptionType;
            return this;
        }

        /// <summary>
        /// Sets the type of aggregate assertion exception.
        /// The default value is a type of <see cref="AggregateAssertionException"/>.
        /// The exception type should have public constructor with <c>IEnumerable&lt;AssertionResult&gt;</c> argument.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseAggregateAssertionExceptionType<TException>()
            where TException : Exception
        {
            return UseAggregateAssertionExceptionType(typeof(TException));
        }

        /// <summary>
        /// Sets the type of aggregate assertion exception.
        /// The default value is a type of <see cref="AggregateAssertionException"/>.
        /// The exception type should have public constructor with <c>IEnumerable&lt;AssertionResult&gt;</c> argument.
        /// </summary>
        /// <param name="exceptionType">The type of the exception.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseAggregateAssertionExceptionType(Type exceptionType)
        {
            exceptionType.CheckIs<Exception>(nameof(exceptionType));

            BuildingContext.AggregateAssertionExceptionType = exceptionType;
            return this;
        }

        /// <summary>
        /// Sets the default assembly name pattern that is used to filter assemblies to find types in them.
        /// Modifies the <see cref="AtataBuildingContext.DefaultAssemblyNamePatternToFindTypes"/> property value of <see cref="BuildingContext"/>.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseDefaultAssemblyNamePatternToFindTypes(string pattern)
        {
            pattern.CheckNotNullOrWhitespace(nameof(pattern));

            BuildingContext.DefaultAssemblyNamePatternToFindTypes = pattern;
            return this;
        }

        /// <summary>
        /// Sets the assembly name pattern that is used to filter assemblies to find component types in them.
        /// Modifies the <see cref="AtataBuildingContext.AssemblyNamePatternToFindComponentTypes"/> property value of <see cref="BuildingContext"/>.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseAssemblyNamePatternToFindComponentTypes(string pattern)
        {
            pattern.CheckNotNullOrWhitespace(nameof(pattern));

            BuildingContext.AssemblyNamePatternToFindComponentTypes = pattern;
            return this;
        }

        /// <summary>
        /// Sets the assembly name pattern that is used to filter assemblies to find attribute types in them.
        /// Modifies the <see cref="AtataBuildingContext.AssemblyNamePatternToFindAttributeTypes"/> property value of <see cref="BuildingContext"/>.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseAssemblyNamePatternToFindAttributeTypes(string pattern)
        {
            pattern.CheckNotNullOrWhitespace(nameof(pattern));

            BuildingContext.AssemblyNamePatternToFindAttributeTypes = pattern;
            return this;
        }

        /// <summary>
        /// Adds the action to perform during <see cref="AtataContext"/> building.
        /// It will be executed at the beginning of the build after the log is set up.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder OnBuilding(Action action)
        {
            if (action != null)
                BuildingContext.OnBuildingActions.Add(action);
            return this;
        }

        /// <summary>
        /// Adds the action to perform after <see cref="AtataContext"/> building.
        /// It will be executed at the end of the build after the driver is created.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder OnBuilt(Action action)
        {
            if (action != null)
                BuildingContext.OnBuiltActions.Add(action);
            return this;
        }

        /// <summary>
        /// Adds the action to perform after the driver is created.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder OnDriverCreated(Action<RemoteWebDriver> action)
        {
            if (action != null)
                BuildingContext.OnDriverCreatedActions.Add(action);
            return this;
        }

        /// <summary>
        /// Adds the action to perform after the driver is created.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder OnDriverCreated(Action action)
        {
            return action != null ? OnDriverCreated(_ => action()) : this;
        }

        /// <summary>
        /// Adds the action to perform during <see cref="AtataContext"/> cleanup.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder OnCleanUp(Action action)
        {
            if (action != null)
                BuildingContext.CleanUpActions.Add(action);
            return this;
        }

        /// <summary>
        /// Defines that the name of the test should be taken from the NUnit test.
        /// </summary>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseNUnitTestName()
        {
            return UseTestName(ResolveNUnitTestName);
        }

        private static string ResolveNUnitTestName()
        {
            dynamic testContext = GetNUnitTestContext();
            return testContext.Test.Name;
        }

        /// <summary>
        /// Sets <see cref="NUnitAggregateAssertionStrategy"/> as the aggregate assertion strategy.
        /// The <see cref="NUnitAggregateAssertionStrategy"/> uses NUnit's <c>Assert.Multiple</c> method for aggregate assertion.
        /// </summary>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseNUnitAggregateAssertionStrategy()
        {
            return UseAggregateAssertionStrategy(new NUnitAggregateAssertionStrategy());
        }

        /// <summary>
        /// Sets the aggregate assertion strategy.
        /// </summary>
        /// <typeparam name="TAggregateAssertionStrategy">The type of the aggregate assertion strategy.</typeparam>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseAggregateAssertionStrategy<TAggregateAssertionStrategy>()
            where TAggregateAssertionStrategy : IAggregateAssertionStrategy, new()
        {
            IAggregateAssertionStrategy strategy = Activator.CreateInstance<TAggregateAssertionStrategy>();

            return UseAggregateAssertionStrategy(strategy);
        }

        /// <summary>
        /// Sets the aggregate assertion strategy.
        /// </summary>
        /// <param name="strategy">The aggregate assertion strategy.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseAggregateAssertionStrategy(IAggregateAssertionStrategy strategy)
        {
            BuildingContext.AggregateAssertionStrategy = strategy;

            return this;
        }

        /// <summary>
        /// Sets <see cref="NUnitWarningReportStrategy"/> as the strategy for warning assertion reporting.
        /// </summary>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseNUnitWarningReportStrategy()
        {
            return UseWarningReportStrategy(new NUnitWarningReportStrategy());
        }

        /// <summary>
        /// Sets the strategy for warning assertion reporting.
        /// </summary>
        /// <param name="strategy">The warning report strategy.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseWarningReportStrategy(IWarningReportStrategy strategy)
        {
            BuildingContext.WarningReportStrategy = strategy;

            return this;
        }

        /// <summary>
        /// Defines that an error occurred during the NUnit test execution should be added to the log during the cleanup.
        /// </summary>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder LogNUnitError()
        {
            return OnCleanUp(() =>
            {
                dynamic testResult = GetNUnitTestResult();

                if (IsNUnitTestResultFailed(testResult))
                    AtataContext.Current.Log.Error((string)testResult.Message, (string)testResult.StackTrace);
            });
        }

        /// <summary>
        /// Defines that an error occurred during the NUnit test execution should be captured by a screenshot during the cleanup.
        /// </summary>
        /// <param name="title">The screenshot title.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder TakeScreenshotOnNUnitError(string title = "Failed")
        {
            return OnCleanUp(() =>
            {
                dynamic testResult = GetNUnitTestResult();

                if (IsNUnitTestResultFailed(testResult))
                    AtataContext.Current.Log.Screenshot(title);
            });
        }

        /// <summary>
        /// Sets the type of <c>NUnit.Framework.AssertionException</c> as the assertion exception type.
        /// The default value is a type of <see cref="AssertionException"/>.
        /// </summary>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseNUnitAssertionExceptionType()
        {
            return UseAssertionExceptionType(NUnitAdapter.AssertionExceptionType);
        }

        /// <summary>
        /// Enables all NUnit features for Atata.
        /// Executes the following methods:
        /// <list type="bullet">
        /// <item><see cref="UseNUnitTestName"/>,</item>
        /// <item><see cref="UseNUnitAssertionExceptionType"/>,</item>
        /// <item><see cref="UseNUnitAggregateAssertionStrategy"/>,</item>
        /// <item><see cref="UseNUnitWarningReportStrategy"/>,</item>
        /// <item><see cref="AddNUnitTestContextLogging"/>,</item>
        /// <item><see cref="LogNUnitError"/>,</item>
        /// <item><see cref="TakeScreenshotOnNUnitError(string)"/>.</item>
        /// </list>
        /// </summary>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseAllNUnitFeatures()
        {
            return UseNUnitTestName().
                UseNUnitAssertionExceptionType().
                UseNUnitAggregateAssertionStrategy().
                UseNUnitWarningReportStrategy().
                AddNUnitTestContextLogging().
                LogNUnitError().
                TakeScreenshotOnNUnitError();
        }

        private static dynamic GetNUnitTestContext()
        {
            Type testContextType = Type.GetType("NUnit.Framework.TestContext,nunit.framework", true);
            PropertyInfo currentContextProperty = testContextType.GetPropertyWithThrowOnError("CurrentContext");

            return currentContextProperty.GetStaticValue();
        }

        private static dynamic GetNUnitTestResult()
        {
            dynamic testContext = GetNUnitTestContext();
            return testContext.Result;
        }

        private static bool IsNUnitTestResultFailed(dynamic testResult)
        {
            return testResult.Outcome.Status.ToString().Contains("Fail");
        }

        /// <summary>
        /// Clears the <see cref="BuildingContext"/>.
        /// </summary>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder Clear()
        {
            BuildingContext = new AtataBuildingContext();
            return this;
        }

        /// <summary>
        /// Builds the <see cref="AtataContext" /> instance and sets it to <see cref="AtataContext.Current" /> property.
        /// </summary>
        /// <returns>The created <see cref="AtataContext"/> instance.</returns>
        public AtataContext Build()
        {
            AtataContext.InitGlobalVariables();

            ValidateBuildingContextBeforeBuild();

            LogManager logManager = new LogManager();

            foreach (var logConsumerItem in BuildingContext.LogConsumers)
                logManager.Use(logConsumerItem.Consumer, logConsumerItem.MinLevel, logConsumerItem.LogSectionFinish);

            foreach (var screenshotConsumer in BuildingContext.ScreenshotConsumers)
                logManager.Use(screenshotConsumer);

            AtataContext context = new AtataContext
            {
                TestName = BuildingContext.TestNameFactory?.Invoke(),
                BaseUrl = BuildingContext.BaseUrl,
                Log = logManager,
                OnDriverCreatedActions = BuildingContext.OnDriverCreatedActions?.ToList() ?? new List<Action<RemoteWebDriver>>(),
                CleanUpActions = BuildingContext.CleanUpActions?.ToList() ?? new List<Action>(),
                Attributes = BuildingContext.Attributes.Clone(),
                BaseRetryTimeout = BuildingContext.BaseRetryTimeout,
                BaseRetryInterval = BuildingContext.BaseRetryInterval,
                ElementFindTimeout = BuildingContext.ElementFindTimeout,
                ElementFindRetryInterval = BuildingContext.ElementFindRetryInterval,
                WaitingTimeout = BuildingContext.WaitingTimeout,
                WaitingRetryInterval = BuildingContext.WaitingRetryInterval,
                VerificationTimeout = BuildingContext.VerificationTimeout,
                VerificationRetryInterval = BuildingContext.VerificationRetryInterval,
                Culture = BuildingContext.Culture ?? CultureInfo.CurrentCulture,
                AssertionExceptionType = BuildingContext.AssertionExceptionType,
                AggregateAssertionExceptionType = BuildingContext.AggregateAssertionExceptionType,
                AggregateAssertionStrategy = BuildingContext.AggregateAssertionStrategy ?? new AtataAggregateAssertionStrategy(),
                WarningReportStrategy = BuildingContext.WarningReportStrategy ?? new AtataWarningReportStrategy()
            };

            AtataContext.Current = context;

            OnBuilding(context);

            if (context.BaseUrl != null)
                context.Log.Trace($"Set: BaseUrl={context.BaseUrl}");

            LogRetrySettings(context);

            if (BuildingContext.Culture != null)
                ApplyCulture(context, BuildingContext.Culture);

            context.DriverFactory = BuildingContext.DriverFactoryToUse;
            context.DriverAlias = BuildingContext.DriverFactoryToUse.Alias;

            context.InitDriver();

            context.Log.Trace($"Set: Driver={context.Driver.GetType().Name}{BuildingContext.DriverFactoryToUse?.Alias?.ToFormattedString(" (alias={0})")}");

            OnBuilt(context);

            return context;
        }

        private void OnBuilding(AtataContext context)
        {
            context.LogTestStart();

            context.Log.Start("Set up AtataContext", LogLevel.Trace);

            if (BuildingContext.OnBuildingActions != null)
            {
                foreach (Action action in BuildingContext.OnBuildingActions)
                {
                    try
                    {
                        action();
                    }
                    catch (Exception e)
                    {
                        context.Log.Error($"On {nameof(AtataContext)} building action failure.", e);
                    }
                }
            }
        }

        private void OnBuilt(AtataContext context)
        {
            if (BuildingContext.OnBuiltActions != null)
            {
                foreach (Action action in BuildingContext.OnBuiltActions)
                {
                    try
                    {
                        action();
                    }
                    catch (Exception e)
                    {
                        context.Log.Error($"On {nameof(AtataContext)} built action failure.", e);
                    }
                }
            }

            context.Log.EndSection();

            context.CleanExecutionStartDateTime = DateTime.Now;
        }

        private static void LogRetrySettings(AtataContext context)
        {
            string messageFormat = "Set: {0}Timeout={1}; {0}RetryInterval={2}";

            context.Log.Trace(messageFormat, "ElementFind", context.ElementFindTimeout.ToShortIntervalString(), context.ElementFindRetryInterval.ToShortIntervalString());
            context.Log.Trace(messageFormat, "Waiting", context.WaitingTimeout.ToShortIntervalString(), context.WaitingRetryInterval.ToShortIntervalString());
            context.Log.Trace(messageFormat, "Verification", context.VerificationTimeout.ToShortIntervalString(), context.VerificationRetryInterval.ToShortIntervalString());
        }

        private static void ApplyCulture(AtataContext context, CultureInfo culture)
        {
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = culture;

#if !NET40
            if (AtataContext.ModeOfCurrent == AtataContextModeOfCurrent.Static)
                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = culture;
#endif

            context.Log.Trace($"Set: Culture={culture.Name}");
        }

        private void ValidateBuildingContextBeforeBuild()
        {
            if (BuildingContext.DriverFactoryToUse == null)
            {
                throw new InvalidOperationException(
                    $"Cannot build {nameof(AtataContext)} as no driver is specified. " +
                    $"Use one of \"Use*\" methods to specify the driver to use, e.g.: AtataContext.Configure().UseChrome().Build();");
            }
        }
    }
}
