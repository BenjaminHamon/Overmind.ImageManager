import commands.artifact
import commands.clean
import commands.compile
import commands.metadata


def configure_argument_parser(environment, configuration, subparsers): # pylint: disable = unused-argument
	return subparsers.add_parser("release", help = "build a package for release")


def run(environment, configuration, arguments): # pylint: disable = unused-argument
	arguments.configuration = "Release"
	arguments.parameters = { }
	arguments.artifact = "package_final"
	arguments.artifact_commands = [ "package", "verify" ]

	if arguments.simulate:
		arguments.artifact_commands = [ "package" ]

	commands.clean.run(environment, configuration, arguments)
	print("")
	commands.metadata.run(environment, configuration, arguments)
	print("")
	commands.compile.run(environment, configuration, arguments)
	print("")
	commands.artifact.run(environment, configuration, arguments)
