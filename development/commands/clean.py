import logging
import os
import shutil


logger = logging.getLogger("Main")


def configure_argument_parser(environment, configuration, subparsers): # pylint: disable = unused-argument
	return subparsers.add_parser("clean", help = "clean the workspace")


def run(environment, configuration, arguments): # pylint: disable = unused-argument
	clean(arguments.simulate)


def clean(simulate):
	logger.info("Cleaning the workspace")
	print("")

	directories_to_clean = [
		{ "display_name": "NuGet packages", "path": "packages" },
		{ "display_name": "Build", "path": ".build" },
		{ "display_name": "Build artifacts", "path": ".artifacts" },
		{ "display_name": "Test Results", "path": "TestResults" },
	]

	for directory in directories_to_clean:
		if os.path.exists(directory["path"]):
			logger.info("Removing directory '%s' (Path: %s)", directory["display_name"], directory["path"])
			if not simulate:
				shutil.rmtree(directory["path"])
