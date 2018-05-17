import json
import logging
import os
import subprocess


def configure_logging(log_level):
	log_format = "[{levelname}] {message}"
	logging.basicConfig(level = log_level, format = log_format, style = "{")
	logging.addLevelName(logging.DEBUG, "Debug")
	logging.addLevelName(logging.INFO, "Info")
	logging.addLevelName(logging.WARNING, "Warning")
	logging.addLevelName(logging.ERROR, "Error")
	logging.addLevelName(logging.CRITICAL, "Critical")


def create_default_environment():
	return {
		"git_executable": "git",
		"msbuild_2017_executable": "msbuild_2017",
		"nuget_executable": "nuget",
		"python3_executable": "python3",
	}


def load_environment():
	env = create_default_environment()
	env.update(_load_environment_transform(os.path.join(os.path.expanduser("~"), ".overmind", "environment.json")))
	env.update(_load_environment_transform("environment.json"))
	return env


def _load_environment_transform(transform_file_path):
	if not os.path.exists(transform_file_path):
		return {}
	with open(transform_file_path) as transform_file:
		return json.load(transform_file)


def get_version(env):
	with open("Version.json", "r") as version_file:
		version = json.loads(version_file.read())
	version["revision"] = subprocess.check_output([ env["git_executable"], "rev-parse", "--verify", "--short=10", "HEAD" ]).decode("utf-8").strip()
	version["branch"] = subprocess.check_output([ env["git_executable"], "rev-parse", "--abbrev-ref", "HEAD" ]).decode("utf-8").strip()
	version["full"] = version["full_format"].format(**version)
	version["numeric"] = version["numeric_format"].format(**version)
	return version
