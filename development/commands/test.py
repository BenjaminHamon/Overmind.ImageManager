import logging
import shutil
import subprocess


logger = logging.getLogger("Main")


def configure_argument_parser(environment, configuration, subparsers): # pylint: disable = unused-argument
	parser = subparsers.add_parser("test", help = "run the test suite")
	parser.add_argument("--configuration", required = True, choices = configuration["compilation_configurations"],
		type = lambda s: s.lower(), help = "set the solution configuration")
	parser.add_argument("--filter", metavar = "<expression>", help = "specify an expression to select tests to run")
	return parser


def run(environment, configuration, arguments): # pylint: disable = unused-argument
	vstest_executable = environment.get("vstest_executable", None)
	if vstest_executable is None or not shutil.which(vstest_executable):
		raise RuntimeError("VSTest is required (Path: '%s')" % vstest_executable)

	test_container = ".build/{assembly}/Binaries/{configuration}/{project}.{assembly}.dll"
	test_container = test_container.format(project = configuration["project"], assembly = "Test", configuration = arguments.configuration)
	test(vstest_executable, test_container, arguments.configuration, arguments.filter)


def test(vstest_executable, test_container, configuration, filter_expression):
	logger.info("Running test suite (Configuration: '%s')", configuration)

	vstest_command = [ vstest_executable, "/Logger:trx" ]
	if filter_expression:
		vstest_command += [ "/TestCaseFilter:" + filter_expression ]
	vstest_command += [ test_container ]

	logger.info("+ %s", " ".join(("'" + x + "'") if " " in x else x for x in vstest_command))
	subprocess.check_call(vstest_command)
