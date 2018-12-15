import logging

import artifact
import clean
import compile
import metadata


def configure_argument_parser(environment, configuration, subparsers):
	return subparsers.add_parser("release", help = "build a package for release")


def run(environment, configuration, arguments):
	arguments.configuration = "Release"
	arguments.parameters = { }
	arguments.artifact = "package_final"
	arguments.artifact_commands = [ "package", "verify" ]

	if arguments.simulate:
		arguments.artifact_commands = [ "package" ]

	clean.run(environment, configuration, arguments)
	logging.info("")
	metadata.run(environment, configuration, arguments)
	logging.info("")
	compile.run(environment, configuration, arguments)
	logging.info("")
	artifact.run(environment, configuration, arguments)
