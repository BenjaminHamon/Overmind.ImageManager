#!/usr/bin/env python3


import argparse
import glob
import logging
import os
import shutil
import subprocess
import zipfile

import environment


verbose = False
test_run = False
env = {}

project = "Overmind.ImageManager"
project_name = "Overmind Image Manager"
project_version = None
application_list = [ "WindowsClient", "WallpaperService" ]
configuration_list = [ "Debug", "Release" ]
configuration = "Debug"


def parse_arguments():
	parser = argparse.ArgumentParser()
	available_command_list = [ "clean", "build", "package" ]
	parser.add_argument("--command", default = [], nargs = "+", choices = available_command_list, help = "action(s) to execute on the project")
	parser.add_argument("--configuration", default = "Debug", choices = configuration_list, help = "set the build and publish configuration")
	parser.add_argument("--test-run", action = "store_true", help = "perform a test run, without actually executing commands")
	parser.add_argument("--verbose", action = "store_true", help = "increase output verbosity")
	return parser.parse_args()


def execute_commands(command_list):
	if "clean" in command_list:
		clean()
	if "build" in command_list:
		build()
	if "package" in command_list:
		package()


def show_project_information():
	logging.info("%s %s (Configuration: %s)", project_name, project_version["full"], configuration)
	logging.info("Script executing in %s %s", os.getcwd(), "(TEST RUN)" if test_run else '')
	logging.info("")


def clean():
	logging.info("=== Clean ===")

	directories_to_clean = [
		{ "name": "NuGet packages", "path": "packages" },
		{ "name": "Build artifacts", "path": ".build" },
	]

	filesets_to_clean = [
		{ "name": "Generated files", "files": [ "ProductInformation.cs" ] },
	]

	for directory in directories_to_clean:
		if os.path.exists(directory["path"]):
			logging.info("Removing directory %s (Path: %s)", directory["name"], directory["path"])
			if not test_run:
			 	shutil.rmtree(directory["path"])

	for fileset in filesets_to_clean:
		logging.info("Removing fileset %s", fileset["name"])
		for file_path in fileset["files"]:
			if os.path.exists(file_path):
				logging.info("  Removing %s", file_path)
				if not test_run:
			 		os.remove(file_path)

	logging.info("")


def build():
	logging.info("=== Build ===")

	nuget_command = [ env["nuget_executable"], "restore" ]
	if verbose == False:
		nuget_command += [ "-Verbosity", "quiet" ]
	nuget_command += [ project + ".sln" ]
	logging.info("+ %s", " ".join(nuget_command))
	if not test_run:
		subprocess.check_call(nuget_command)
		logging.debug("")

	msbuild_command = [ env["msbuild_2017_executable"], "/m", "/nologo" ]
	if verbose == False:
		msbuild_command += [ "/v:Minimal" ]
	msbuild_command += [ "/target:build" ]
	msbuild_command += [ "/p:Configuration=" + configuration ]
	msbuild_command += [ "/p:PythonExecutable=" + env["python3_executable"] ]
	msbuild_command += [ project + ".sln" ]
	logging.info("+ %s", " ".join(msbuild_command))
	if not test_run:
		subprocess.check_call(msbuild_command)

	logging.info("")


def package():
	logging.info("=== Package ===")

	for application in application_list:
		source_directory = ".build/" + application + "/bin/" + configuration
		destination_directory = ".build/" + application + "/packages"
		package_information = { "project": project, "application": application, "version": project_version["full"], "configuration": configuration }
		package_name = "{project}.{application}_{version}_{configuration}.zip".format(**package_information)

		if not test_run and not os.path.exists(destination_directory):
			os.makedirs(destination_directory)

		logging.info("Creating package for %s (Path: %s)", application, destination_directory + "/" + package_name)
		source_file_list = fileset_to_list(source_directory, get_package_fileset(application, configuration))
		if not source_file_list:
			raise Exception("Package files could not be found (Source: %s)" % source_directory)
		if test_run:
			for source_file in source_file_list:
				logging.debug("  Adding %s", source_file)
		else:
			with zipfile.ZipFile(destination_directory + "/" + package_name + ".tmp", "w", zipfile.ZIP_DEFLATED) as package_file:
				for source_file in source_file_list:
					logging.debug("  Adding %s", source_file)
					package_file.write(source_file, project + "." + application + "/" + source_file[len(source_directory) + 1 : ])
			shutil.move(destination_directory + "/" + package_name + ".tmp", destination_directory + "/" + package_name)

		logging.debug("")

	if not verbose:
		logging.info("")


def fileset_to_list(directory, fileset):
	all_files = []
	for file_pattern in fileset:
		matched_file_list = glob.glob(directory + "/" + file_pattern)
		if not matched_file_list:
			logging.info("No matches for %s in %s", file_pattern, directory)
		all_files += matched_file_list
	return all_files


def get_package_fileset(application, configuration):
	fileset = [ project + "." + application + ".exe", project + "." + application + ".exe.config", "*.dll" ]
	if configuration == "Debug":
		fileset += [ "*.pdb", "*.xml" ]
	if application == "WindowsClient":
		fileset += [ "Credits.txt" ]
	return fileset


if __name__ == "__main__":
	base_directory = os.path.dirname(os.path.realpath(__file__))
	os.chdir(base_directory)

	arguments = parse_arguments()
	verbose = arguments.verbose
	test_run = arguments.test_run
	configuration = arguments.configuration

	environment.configure_logging(logging.DEBUG if verbose else logging.INFO)
	env = environment.load_environment()
	project_version = environment.get_version(env)

	show_project_information()
	execute_commands(arguments.command)
