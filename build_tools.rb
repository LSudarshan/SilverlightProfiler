def msbuild(solution, opts={})
  solution_dir = opts.has_key?(:path) ? opts[:path] : @props[:solution_path]
  solution_path = File.join(solution_dir, "#{solution}.sln")
  sh "#{@props[:tools][:msbuild]} #{solution_path}"
end

def nunit(proj)
  sh "#{@props[:tools][:nunit]} #{proj}\\bin\\debug\\#{proj}.dll"
end

def vdir(path, name)
  output = `#{@props[:tools][:appcmd]} list apps`
  if output.include? name
      sh "#{@props[:tools][:appcmd]} delete app \"Default Web Site/#{name}\""
  end
  sh "#{@props[:tools][:appcmd]} add app /site.name:\"Default Web Site\" /path:\"/#{name}\" /physicalPath:\"#{path}\""
end

def copy_files from, to
  FileList[from].each do |f|
    cp f, "#{to}\\#{File.basename(f)}"
  end
end

def setup_defaults
  @props = {}
  @props[:solution_path] = '.\\'
  @props[:tools] = {}
  @props[:tools][:msbuild] = 'C:\\WINDOWS\\Microsoft.NET\\Framework\\v4.0.30319\\MSBuild.exe'
  @props[:tools][:nunit] = 'tools\\NUnit\\nunit-console.exe'
  @props[:tools][:appcmd] = 'C:\Windows\System32\inetsrv\appcmd.exe'
end

setup_defaults