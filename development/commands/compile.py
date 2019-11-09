import logging
import subprocess


logger = logging.getLogger("Main")


def configure_argument_parser(environment, configuration, subparsers): # pylint: disable = unused-argument
	parser = subparsers.add_parser("compile", help = "compile the project")
	parser.add_argument("--configuration", required = True, choices = configuration["compilation_configurations"],
		type = lambda s: s.lower(), help = "set the solution configuration")
	return parser


def run(environment, configuration, arguments): # pylint: disable = unused-argument
	solution = configuration["project"] + ".sln"
	compile(environment, solution, arguments.configuration, arguments.verbosity == "debug", arguments.simulate)


def compile(environment, solution, configuration, verbose, simulate): # pylint: disable = redefined-builtin
	logger.info("Compiling %s with configuration %s", solution, configuration)
	print("")

	nuget_command = [ environment["nuget_executable"], "restore" ]
	if not verbose:
		nuget_command += [ "-Verbosity", "quiet" ]
	nuget_command += [ solution ]

	logger.info("+ %s", " ".join(("'" + x + "'") if " " in x else x for x in nuget_command))
	if not simulate:
		subprocess.check_call(nuget_command)
		if verbose:
			print("")

	msbuild_command = [ environment["msbuild_2017_executable"], "/m", "/NoLogo" ]
	if not verbose:
		msbuild_command += [ "/v:Minimal" ]
	msbuild_command += [ "/target:build" ]
	msbuild_command += [ "/p:Configuration=" + configuration ]
	msbuild_command += [ solution ]

	logger.info("+ %s", " ".join(("'" + x + "'") if " " in x else x for x in msbuild_command))
	if not simulate:
		subprocess.check_call(msbuild_command)
