import subprocess

import artifact
import clean
import compile
import metadata
import release


def get_command_list():
	return [ artifact, clean, compile, metadata, release ]


def load_configuration(environment):
	configuration = {
		"project": "Overmind.ImageManager",
		"project_name": "Overmind Image Manager",
		"project_version": { "identifier": "2.0" },

		"configuration_list": [ "Debug", "Release" ],
	}

	configuration["project_version"]["revision"] = subprocess.check_output([ environment["git_executable"], "rev-parse", "--short=10", "HEAD" ]).decode("utf-8").strip()
	configuration["project_version"]["branch"] = subprocess.check_output([ environment["git_executable"], "rev-parse", "--abbrev-ref", "HEAD" ]).decode("utf-8").strip()
	configuration["project_version"]["numeric"] = "{identifier}".format(**configuration["project_version"])
	configuration["project_version"]["full"] = "{identifier}-{revision}".format(**configuration["project_version"])

	configuration["filesets"] = {

		# Program binaries for development (executables, libraries, symbols and documentation)
		"binaries": {
			"path_in_workspace": ".build/{assembly}/bin/{configuration}",
			"file_patterns": [ "{project}.{assembly}.exe", "{project}.{assembly}.exe.config", "*.dll", "*.pdb", "*.xml" ],
		},

		# Program binaries for release (executables and libraries)
		"binaries_stripped": {
			"path_in_workspace": ".build/{assembly}/bin/{configuration}",
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
			"path_in_repository": "binaries",
			"filesets": [
				{ "identifier": "binaries", "path_in_archive": "." },
			],
		},

		# Development package
		"package": {
			"file_name": "{project}_{version}_Package_{configuration}",
			"path_in_repository": "packages",
			"filesets": [
				{ "identifier": "binaries", "path_in_archive": ".", "parameters": { "assembly": "WallpaperService" } },
				{ "identifier": "binaries", "path_in_archive": ".", "parameters": { "assembly": "WindowsClient" } },
				{ "identifier": "resources", "path_in_archive": "." },
			],
		},

		# Release package
		"package_final": {
			"file_name": "{project}_{version}_PackageFinal",
			"path_in_repository": "packages",
			"filesets": [
				{ "identifier": "binaries_stripped", "path_in_archive": ".", "parameters": { "assembly": "WallpaperService", "configuration": "Release" } },
				{ "identifier": "binaries_stripped", "path_in_archive": ".", "parameters": { "assembly": "WindowsClient", "configuration": "Release" } },
				{ "identifier": "resources", "path_in_archive": "." },
			],
		},
	}

	return configuration
