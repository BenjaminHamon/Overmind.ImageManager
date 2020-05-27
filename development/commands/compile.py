import logging
import shutil
import subprocess


logger = logging.getLogger("Main")


def configure_argument_parser(environment, configuration, subparsers): # pylint: disable = unused-argument
	parser = subparsers.add_parser("compile", help = "compile the project")
	parser.add_argument("--configuration", required = True, metavar = "<configuration>", help = "set the solution configuration")
	return parser


def run(environment, configuration, arguments): # pylint: disable = unused-argument
	nuget_executable = environment.get("nuget_executable", None)
	if nuget_executable is None or not shutil.which(nuget_executable):
		raise RuntimeError("NuGet is required (Path: '%s')" % nuget_executable)

	msbuild_executable = environment.get("msbuild_executable", None)
	if msbuild_executable is None or not shutil.which(msbuild_executable):
		raise RuntimeError("MSBuild is required (Path: '%s')" % msbuild_executable)

	compile(nuget_executable, msbuild_executable, configuration["dotnet_solution"], arguments.configuration, arguments.verbosity == "debug", simulate = arguments.simulate)


def compile( # pylint: disable = redefined-builtin, too-many-arguments
		nuget_executable, msbuild_executable, solution, configuration, verbose, simulate):
	logger.info("Compiling '%s' (Configuration: '%s')", solution, configuration)
	print("")

	nuget_command = [ nuget_executable, "restore" ]
	if not verbose:
		nuget_command += [ "-Verbosity", "quiet" ]
	nuget_command += [ solution ]

	logger.info("+ %s", " ".join(("'" + x + "'") if " " in x else x for x in nuget_command))
	if not simulate:
		subprocess.check_call(nuget_command)
		if verbose:
			print("")

	msbuild_command = [ msbuild_executable, "/m", "/NoLogo" ]
	if not verbose:
		msbuild_command += [ "/v:Minimal" ]
	msbuild_command += [ "/target:build" ]
	msbuild_command += [ "/p:Configuration=" + configuration ]
	msbuild_command += [ solution ]

	logger.info("+ %s", " ".join(("'" + x + "'") if " " in x else x for x in msbuild_command))
	if not simulate:
		subprocess.check_call(msbuild_command)
