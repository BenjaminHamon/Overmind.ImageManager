#!/usr/bin/env python3


import argparse
import glob
import os
import shutil
import subprocess
import zipfile

import environment


verbose = False
test_run = False

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
	print("{project_name} {project_version[full]} (Configuration: {configuration})".format(**globals()))
	print("Script executing in {directory} {test_run}".format(directory = os.getcwd(), test_run = "(TEST RUN)" if test_run else ''))
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


def package():
	print("=== Package ===")

	for application in application_list:
		source_directory = ".build/" + application + "/bin/" + configuration
		destination_directory = ".build/" + application + "/packages"
		package_information = { "project": project, "application": application, "version": project_version["full"], "configuration": configuration.lower() }
		package_name = "{project}.{application}-{version}-{configuration}.zip".format(**package_information)

		if not test_run and not os.path.exists(destination_directory):
			os.makedirs(destination_directory)

		print("Packaging {application} to {package_path}".format(application = application, package_path = destination_directory + "/" + package_name))
		source_file_list = fileset_to_list(source_directory, get_package_fileset(application, configuration))
		if not source_file_list:
			raise Exception("Package files could not be found ({directory})".format(directory = source_directory))
		if test_run and verbose:
			for source_file in source_file_list:
				print("  Adding {file}".format(file = source_file))
		else:
			with zipfile.ZipFile(destination_directory + "/" + package_name + ".tmp", "w", zipfile.ZIP_DEFLATED) as package_file:
				for source_file in source_file_list:
					if verbose:
						print("  Adding {file}".format(file = source_file))
					package_file.write(source_file, project + "." + application + "/" + source_file[len(source_directory) + 1 : ])
			shutil.move(destination_directory + "/" + package_name + ".tmp", destination_directory + "/" + package_name)

		if verbose:
			print()

	if not verbose:
		print()


def fileset_to_list(directory, fileset):
	all_files = []
	for file_pattern in fileset:
		matched_file_list = glob.glob(directory + "/" + file_pattern)
		if not matched_file_list:
			print("No matches for {pattern}".format(pattern = file_pattern))
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

	project_version = environment.get_version()
	show_project_information()
	execute_commands(arguments.command)