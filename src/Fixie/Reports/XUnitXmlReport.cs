﻿using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Fixie.Execution;

namespace Fixie.Reports
{
    public class XUnitXmlReport
    {
        public XDocument Transform(ExecutionResult executionResult)
        {
            return new XDocument(
                new XElement("assemblies",
                    executionResult.AssemblyResults.Select(Assembly)));
        }

        static XElement Assembly(AssemblyResult assemblyResult)
        {
            var now = DateTime.UtcNow;

            var classResults = assemblyResult.ConventionResults.SelectMany(x => x.ClassResults);

            return new XElement("assembly",
                new XAttribute("name", assemblyResult.Name),
                new XAttribute("run-date", now.ToString("yyyy-MM-dd")),
                new XAttribute("run-time", now.ToString("HH:mm:ss")),
                new XAttribute("configFile", AppDomain.CurrentDomain.SetupInformation.ConfigurationFile),
                new XAttribute("time", Seconds(assemblyResult.Duration)),
                new XAttribute("total", assemblyResult.Total),
                new XAttribute("passed", assemblyResult.Passed),
                new XAttribute("failed", assemblyResult.Failed),
                new XAttribute("skipped", assemblyResult.Skipped),
                new XAttribute("environment", String.Format("{0}-bit .NET {1}", IntPtr.Size * 8, Environment.Version)),
                new XAttribute("test-framework", Framework.Version),
                classResults.Select(Class));
        }

        private static XElement Class(ClassResult classResult)
        {
            return new XElement("class",
                new XAttribute("time", Seconds(classResult.Duration)),
                new XAttribute("name", classResult.Name),
                new XAttribute("total", classResult.Failed + classResult.Passed + classResult.Skipped),
                new XAttribute("passed", classResult.Passed),
                new XAttribute("failed", classResult.Failed),
                new XAttribute("skipped", classResult.Skipped),
                classResult.CaseResults.Select(Case));
        }

        private static XElement Case(CaseResult caseResult)
        {
            var @case = new XElement("test",
                new XAttribute("name", caseResult.Name),
                new XAttribute("type", caseResult.MethodGroup.Class),
                new XAttribute("method", caseResult.MethodGroup.Method),
                new XAttribute("result",
                    caseResult.Status == CaseStatus.Failed
                        ? "Fail"
                        : caseResult.Status == CaseStatus.Passed
                            ? "Pass"
                            : "Skip"));

            if (caseResult.Status != CaseStatus.Skipped)
                @case.Add(new XAttribute("time", Seconds(caseResult.Duration)));

            if (caseResult.Status == CaseStatus.Skipped && caseResult.SkipReason != null)
                @case.Add(new XElement("reason", new XElement("message", new XCData(caseResult.SkipReason))));

            if (caseResult.Status == CaseStatus.Failed)
                @case.Add(
                    new XElement("failure",
                        new XAttribute("exception-type", caseResult.Exceptions.PrimaryException.Type),
                        new XElement("message", new XCData(caseResult.Exceptions.PrimaryException.Message)),
                        new XElement("stack-trace", new XCData(caseResult.Exceptions.CompoundStackTrace))));

            return @case;
        }

        static string Seconds(TimeSpan duration)
        {
            //The XML Schema spec requires decimal values to use a culture-ignorant format.
            return duration.TotalSeconds.ToString("0.000", NumberFormatInfo.InvariantInfo);
        }
    }
}