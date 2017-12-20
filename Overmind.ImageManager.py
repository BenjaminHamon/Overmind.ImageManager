#!/usr/bin/env python3


import argparse
import json
import os
import shutil
import subprocess


git_executable = "git"
msbuild_2017_executable = "msbuild_2017"
nuget_executable = "nuget"


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


def get_version():
	with open("Version.json", "r") as version_file:
		version = json.loads(version_file.read())
	version["revision"] = subprocess.check_output([ git_executable, "rev-parse", "--verify", "--short=10", "HEAD" ]).decode("utf-8").strip()
	version["branch"] = subprocess.check_output([ git_executable, "rev-parse", "--abbrev-ref", "HEAD" ]).decode("utf-8").strip()
	version["full"] = version["full_format"].format(**version)
	version["numeric"] = version["numeric_format"].format(**version)
	return version


def show_project_information():
	print("{project_name} {project_version[full]} (Configuration: {configuration})".format(**globals()))
	print("Script executing in {directory}".format(directory = os.getcwd()))
	print()


def clean():
	print("=== Clean ===")
	directories_to_clean = [ "packages", ".build" ]
	for directory in directories_to_clean:
		if os.path.exists(directory):
			print("Removing {directory}".format(directory = directory))
			if not test_run:
			 	shutil.rmtree(directory)
	print()


def build():
	print("=== Build ===")

	nuget_command = [ nuget_executable, "restore" ]
	if verbose == False:
		nuget_command += [ "-Verbosity", "quiet" ]
	nuget_command += [ project + ".sln" ]
	print("+ " + " ".join(nuget_command))
	if not test_run:
		subprocess.check_call(nuget_command)
		if verbose:
			print()

	msbuild_command = [ msbuild_2017_executable, "/m", "/nologo" ]
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

	project_version = get_version()
	show_project_information()
	execute_commands(arguments.command)