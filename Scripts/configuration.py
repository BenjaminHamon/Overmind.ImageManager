import datetime
import json
import os
import subprocess

import commands.artifact
import commands.clean
import commands.compile
import commands.metadata
import commands.release
import commands.test


def get_command_list():
	return [
		commands.artifact,
		commands.clean,
		commands.compile,
		commands.metadata,
		commands.release,
		commands.test,
	]


def load_configuration(environment):
	configuration = {
		"project": "Overmind.ImageManager",
		"project_name": "Overmind Image Manager",
		"project_version": { "identifier": "2.0" },
	}

	branch = subprocess.check_output([ environment["git_executable"], "rev-parse", "--abbrev-ref", "HEAD" ]).decode("utf-8").strip()
	revision = subprocess.check_output([ environment["git_executable"], "rev-parse", "--short=10", "HEAD" ]).decode("utf-8").strip()
	revision_date = int(subprocess.check_output([ environment["git_executable"], "show", "--no-patch", "--format=%ct", revision ]).decode("utf-8").strip())
	revision_date = datetime.datetime.utcfromtimestamp(revision_date).replace(microsecond = 0).isoformat() + "Z"

	configuration["project_version"]["branch"] = branch
	configuration["project_version"]["revision"] = revision
	configuration["project_version"]["date"] = revision_date
	configuration["project_version"]["numeric"] = "{identifier}".format(**configuration["project_version"])
	configuration["project_version"]["full"] = "{identifier}-{revision}".format(**configuration["project_version"])

	configuration["author"] = "Benjamin Hamon"
	configuration["author_email"] = "hamon.benjamin@gmail.com"
	configuration["organization"] = ""
	configuration["project_url"] = "https://github.com/BenjaminHamon/Overmind.ImageManager"
	configuration["copyright"] = "Copyright (c) 2019 Benjamin Hamon"

	configuration["compilation_configurations"] = [ "debug", "release" ]

	if "artifact_repository" in environment:
		configuration["artifact_repository"] = os.path.join(os.path.normpath(environment["artifact_repository"]), "ImageManager")

	configuration["filesets"] = {

		# Program binaries for development (executables, libraries, symbols and documentation)
		"binaries": {
			"path_in_workspace": ".build/{assembly}/Binaries/{configuration}",
			"file_patterns": [ "{project}.{assembly}.exe", "{project}.{assembly}.exe.config", "*.dll", "*.pdb", "*.xml" ],
		},

		# Program binaries for release (executables and libraries)
		"binaries_stripped": {
			"path_in_workspace": ".build/{assembly}/Binaries/{configuration}",
			"file_patterns": [ "{project}.{assembly}.exe", "{project}.{assembly}.exe.config", "*.dll" ],
		},

		# Additional resources, not embedded in programs
		"resources": {
			"path_in_workspace": ".",
			"file_patterns": [ "About.md", "License.txt" ],
		},
	}

	configuration["artifacts"] = {

		# Compilation output
		"binaries": {
			"file_name": "{project}.{assembly}_{version}_Binaries_{configuration}",
			"path_in_repository": "Binaries",
			"filesets": [
				{ "identifier": "binaries", "path_in_archive": "." },
			],
		},

		# Development package
		"package": {
			"file_name": "{project}_{version}_Package_{configuration}",
			"path_in_repository": "Packages",
			"filesets": [
				{ "identifier": "binaries", "path_in_archive": ".", "parameters": { "assembly": "WallpaperService" } },
				{ "identifier": "binaries", "path_in_archive": ".", "parameters": { "assembly": "WindowsClient" } },
				{ "identifier": "resources", "path_in_archive": "." },
			],
		},

		# Release package
		"package_final": {
			"file_name": "{project}_{version}_PackageFinal",
			"path_in_repository": "Packages",
			"filesets": [
				{ "identifier": "binaries_stripped", "path_in_archive": ".", "parameters": { "assembly": "WallpaperService", "configuration": "Release" } },
				{ "identifier": "binaries_stripped", "path_in_archive": ".", "parameters": { "assembly": "WindowsClient", "configuration": "Release" } },
				{ "identifier": "resources", "path_in_archive": "." },
			],
		},
	}

	return configuration


def load_results(result_file_path):
	if not os.path.isfile(result_file_path):
		return { "artifacts": [] }
	with open(result_file_path, "r") as result_file:
		results = json.load(result_file)
		results["artifacts"] = results.get("artifacts", [])
	return results


def save_results(result_file_path, result_data):
	if os.path.dirname(result_file_path):
		os.makedirs(os.path.dirname(result_file_path), exist_ok = True)
	with open(result_file_path, "w") as result_file:
		json.dump(result_data, result_file, indent = 4)
