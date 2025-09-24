using System;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository;
using log4net.Repository.Hierarchy;

namespace IOITQln.Common.Services
{
    public class LogMaster
    {
        #region Const
        private const string RollingFileAppenderNameDefault = "Rolling";
        #endregion

        #region Constructors
        static LogMaster()
        {
        }
        #endregion


        #region Public Methods
        public static ILog GetLogger(string arg, string name)
        {
            //It will create a repository for each different arg it will receive
            var repositoryName = arg;

            ILoggerRepository repository = null;

            var repositories = LogManager.GetAllRepositories();
            foreach (var loggerRepository in repositories)
            {
                if (loggerRepository.Name.Equals(repositoryName))
                {
                    repository = loggerRepository;
                    break;
                }
            }

            Hierarchy hierarchy = null;
            if (repository == null)
            {
                //Create a new repository
                repository = LogManager.CreateRepository(repositoryName);

                hierarchy = (Hierarchy)repository;
                hierarchy.Root.Additivity = false;

                //Add appenders you need: here I need a rolling file and a memoryappender
                var rollingAppender = GetRollingAppender(repositoryName);
                hierarchy.Root.AddAppender(rollingAppender);

                BasicConfigurator.Configure(repository);
            }

            //Returns a logger from a particular repository;
            //Logger with same name but different repository will log using different appenders
            return LogManager.GetLogger(repositoryName, name);
        }
        #endregion


        #region Private Methods
        private static IAppender GetRollingAppender(string arg)
        {
            var level = Level.All;

            var rollingFileAppenderLayout = new PatternLayout("%date{HH:mm:ss,fff}|T%2thread|%25.25logger|%5.5level| %message%newline");
            rollingFileAppenderLayout.ActivateOptions();

            var rollingFileAppenderName = string.Format("{0}{1}", RollingFileAppenderNameDefault, arg);

            var rollingFileAppender = new RollingFileAppender();
            rollingFileAppender.Name = rollingFileAppenderName;
            rollingFileAppender.Threshold = level;
            rollingFileAppender.CountDirection = 0;
            rollingFileAppender.AppendToFile = true;
            rollingFileAppender.LockingModel = new FileAppender.MinimalLock();
            rollingFileAppender.StaticLogFileName = true;
            rollingFileAppender.RollingStyle = RollingFileAppender.RollingMode.Date;
            rollingFileAppender.DatePattern = "dd.MM.yyyy'.log'";
            rollingFileAppender.Layout = rollingFileAppenderLayout;
            rollingFileAppender.MaximumFileSize = "200MB";
            rollingFileAppender.File = string.Format("log/{0}-{1}.txt", DateTime.Now.ToString("dd.MM.yyyy"), arg);
            rollingFileAppender.ActivateOptions();

            return rollingFileAppender;
        }
        #endregion
    }
}