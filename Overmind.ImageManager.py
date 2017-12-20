#!/usr/bin/env python3


import argparse
import os
import shutil
import subprocess

import environment


verbose = False
test_run = False

project = "Overmind.ImageManager"
project_name = "Overmind Image Manager"
project_version = None
configuration_list = [ "Debug", "Release" ]
configuration = "Debug"


def parse_arguments():
	parser = argparse.ArgumentParser()
	available_command_list = [ "clean", "build" ]
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


def show_project_information():
	print("{project_name} {project_version[full]} (Configuration: {configuration})".format(**globals()))
	print("Script executing in {directory}".format(directory = os.getcwd()))
	print()


def clean():
	print("=== Clean ===")

	directories_to_clean = [
		{ "name": "NuGet packages", "path": "packages" },
		{ "name": "Build artifacts", "path": ".build" },
	]

	filesets_to_clean = [
		{ "name": "Generated files", "files": [ "ProductInformation.cs" ] },
	]

	for directory in directories_to_clean:
		if os.path.exists(directory["path"]):
			print("Removing directory {name} (Path: {path})".format(**directory))
			if not test_run:
			 	shutil.rmtree(directory["path"])

	for fileset in filesets_to_clean:
		print("Removing fileset {name}".format(**fileset))
		for file_path in fileset["files"]:
			if os.path.exists(file_path):
				print("  Removing {file_path}".format(file_path = file_path))
				if not test_run:
			 		os.remove(file_path)

	print()


def build():
	print("=== Build ===")

	nuget_command = [ environment.nuget_executable, "restore" ]
	if verbose == False:
		nuget_command += [ "-Verbosity", "quiet" ]
	nuget_command += [ project + ".sln" ]
	print("+ " + " ".join(nuget_command))
	if not test_run:
		subprocess.check_call(nuget_command)
		if verbose:
			print()

	msbuild_command = [ environment.msbuild_2017_executable, "/m", "/nologo" ]
	if verbose == False:
		msbuild_command += [ "/v:Minimal" ]
	msbuild_command += [ "/target:build" ]
	msbuild_command += [ "/p:Configuration=" + configuration ]
	msbuild_command += [ project + ".sln" ]
	print("+ " + " ".join(msbuild_command))
	if not test_run:
		subprocess.check_call(msbuild_command)

	print()


if __name__ == "__main__":
	base_directory = os.path.dirname(os.path.realpath(__file__))
	os.chdir(base_directory)

	arguments = parse_arguments()
	verbose = arguments.verbose
	test_run = arguments.test_run
	configuration = arguments.configuration

	project_version = environment.get_version()
	show_project_information()
	execute_commands(arguments.command)