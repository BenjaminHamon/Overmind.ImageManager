import development.commands.artifact
import development.commands.clean
import development.commands.compile
import development.commands.metadata


def configure_argument_parser(environment, configuration, subparsers): # pylint: disable = unused-argument
	return subparsers.add_parser("release", help = "build a package for release")


def run(environment, configuration, arguments): # pylint: disable = unused-argument
	arguments.configuration = "Release"
	arguments.parameters = { }
	arguments.artifact = "package_final"
	arguments.artifact_commands = [ "package", "verify" ]

	if arguments.simulate:
		arguments.artifact_commands = [ "package" ]

	development.commands.clean.run(environment, configuration, arguments)
	print("")
	development.commands.metadata.run(environment, configuration, arguments)
	print("")
	development.commands.compile.run(environment, configuration, arguments)
	print("")
	development.commands.artifact.run(environment, configuration, arguments)
