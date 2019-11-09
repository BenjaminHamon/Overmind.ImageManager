import logging
import subprocess


logger = logging.getLogger("Main")


def configure_argument_parser(environment, configuration, subparsers): # pylint: disable = unused-argument
	parser = subparsers.add_parser("test", help = "run the test suite")
	parser.add_argument("--configuration", required = True, choices = configuration["compilation_configurations"],
		type = lambda s: s.lower(), help = "set the solution configuration")
	parser.add_argument("--filter", metavar = "<expression>", help = "specify an expression to select tests to run")
	return parser


def run(environment, configuration, arguments): # pylint: disable = unused-argument
	test_container = ".build/{assembly}/Binaries/{configuration}/{project}.{assembly}.dll"
	test_container = test_container.format(project = configuration["project"], assembly = "Test", configuration = arguments.configuration)
	test(environment, test_container, arguments.filter)


def test(environment, test_container, filter_expression):
	logger.info("Running test suite")

	mstest_command = [ environment["vstest_2017_executable"], "/Logger:trx" ]
	if filter_expression:
		mstest_command += [ "/TestCaseFilter:" + filter_expression ]
	mstest_command += [ test_container ]

	logger.info("+ %s", " ".join(("'" + x + "'") if " " in x else x for x in mstest_command))
	subprocess.check_call(mstest_command)
