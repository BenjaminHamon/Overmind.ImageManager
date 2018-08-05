import logging
import os
import subprocess


def configure_argument_parser(environment, configuration, subparsers):
	parser = subparsers.add_parser("compile", help = "compile the project")
	parser.add_argument("--configuration", required = True, choices = configuration["configuration_list"], help = "set the build configuration")
	return parser


def run(environment, configuration, arguments):
	solution = configuration["project"] + ".sln"
	compile(environment, solution, arguments.configuration, arguments.verbosity == "debug", arguments.simulate)


def compile(environment, solution, configuration, verbose, simulate):
	logging.info("Compiling %s with configuration %s", solution, configuration)
	logging.info("")

	nuget_command = [ environment["nuget_executable"], "restore" ]
	if verbose == False:
		nuget_command += [ "-Verbosity", "quiet" ]
	nuget_command += [ solution ]
	logging.info("+ %s", " ".join(nuget_command))
	if not simulate:
		subprocess.check_call(nuget_command)
		logging.debug("")

	msbuild_command = [ environment["msbuild_2017_executable"], "/m", "/nologo" ]
	if verbose == False:
		msbuild_command += [ "/v:Minimal" ]
	msbuild_command += [ "/target:build" ]
	msbuild_command += [ "/p:Configuration=" + configuration ]
	msbuild_command += [ "/p:PythonExecutable=" + environment["python3_executable"] ]
	msbuild_command += [ solution ]
	logging.info("+ %s", " ".join(msbuild_command))
	if not simulate:
		subprocess.check_call(msbuild_command)