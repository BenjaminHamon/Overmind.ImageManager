import logging
import os
import shutil


def configure_argument_parser(environment, configuration, subparsers):
	return subparsers.add_parser("clean", help = "clean the workspace")


def run(environment, configuration, arguments):
	clean(arguments.simulate)


def clean(simulate):
	logging.info("Cleaning the workspace")
	print("")

	directories_to_clean = [
		{ "display_name": "NuGet packages", "path": "packages" },
		{ "display_name": "Build", "path": ".build" },
		{ "display_name": "Build artifacts", "path": ".artifacts" },
	]

	for directory in directories_to_clean:
		if os.path.exists(directory["path"]):
			logging.info("Removing directory '%s' (Path: %s)", directory["display_name"], directory["path"])
			if not simulate:
			 	shutil.rmtree(directory["path"])
