import json
import logging
import os
import sys


all_log_levels = [ "debug", "info", "warning", "error", "critical" ]


def create_default_environment():
	return {
		"logging_level": "info",
		"logging_format": "[{levelname}][{name}] {message}",
		"logging_date_format": "%Y-%m-%dT%H:%M:%S",

		"git_executable": "git",
		"msbuild_executable": find_msbuild("2019"),
		"nuget_executable": "nuget",
		"scp_executable": "scp",
		"ssh_executable": "ssh",
		"vstest_executable": find_vstest("2019"),
	}


def load_environment():
	env = create_default_environment()
	env.update(_load_environment_transform(os.path.join(os.path.expanduser("~"), "environment.json")))
	env.update(_load_environment_transform("environment.json"))
	return env


def _load_environment_transform(transform_file_path):
	if not os.path.exists(transform_file_path):
		return {}
	with open(transform_file_path) as transform_file:
		return json.load(transform_file)


def configure_logging(environment_instance):
	logging_level = logging.getLevelName(environment_instance["logging_level"].upper())

	logging.root.setLevel(logging_level)

	logging.addLevelName(logging.DEBUG, "Debug")
	logging.addLevelName(logging.INFO, "Info")
	logging.addLevelName(logging.WARNING, "Warning")
	logging.addLevelName(logging.ERROR, "Error")
	logging.addLevelName(logging.CRITICAL, "Critical")

	formatter = logging.Formatter(environment_instance["logging_format"], environment_instance["logging_date_format"], "{")
	stream_handler = logging.StreamHandler(sys.stdout)
	stream_handler.setLevel(logging_level)
	stream_handler.formatter = formatter
	logging.root.addHandler(stream_handler)


def configure_log_file(environment_instance, file_path):
	logging_level = logging.getLevelName(environment_instance["logging_level"].upper())

	formatter = logging.Formatter(environment_instance["logging_format"], environment_instance["logging_date_format"], "{")
	file_handler = logging.FileHandler(file_path, mode = "w")
	file_handler.setLevel(logging_level)
	file_handler.formatter = formatter
	logging.root.addHandler(file_handler)


def find_msbuild(version):
	possible_paths = [
		"C:/Program Files (x86)/Microsoft Visual Studio/%s/Community/MSBuild/Current/Bin/MSBuild.exe" % version,
	]

	for file_path in possible_paths:
		if os.path.exists(file_path):
			return file_path

	return "msbuild"


def find_vstest(version):
	possible_paths = [
		"C:/Program Files (x86)/Microsoft Visual Studio/%s/Community/Common7/IDE/CommonExtensions/Microsoft/TestWindow/VSTest.Console.exe" % version,
	]

	for file_path in possible_paths:
		if os.path.exists(file_path):
			return file_path

	return "vstest"
