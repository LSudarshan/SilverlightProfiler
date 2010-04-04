require 'rexml/document'
require 'FileUtils'
include REXML

path = 'e:\projects\bcg\imd\src\imdclient\imdclient.web\clientbin'


def recreate_dir(name)
  FileUtils.rm_rf name
  Dir.mkdir(name)
end

recreate_dir 'temp'

system("unzip #{path}\\imdclient.xap -d temp")


fail "#{$?}" unless system("silverlightprofiler\\bin\\debug\\silverlightprofiler IMDClient IMDClient.App ApplicationStartup E:\\projects\\NMetrics\\Temp\\
\"IMD || IContact || BCG || Telerik\"")

system('copy /Y SilverlightProfiler\bin\Debug\SilverlightProfilerRuntime.dll temp')

doc = Document.new File.new('temp\appmanifest.xaml')
XPath.first(doc, '//Deployment.Parts').add_element('AssemblyPart', {'x:Name'=>'SilverlightProfilerRuntime', 'Source'=>'SilverlightProfilerRuntime.dll'})

File.open('temp\AppManifest.xaml', 'w') do |f|
  doc.write f, 1
end

Dir.chdir 'temp'

system("del #{path}\\imdclient.xap")
system("zip #{path}\\imdclient.xap *.*")

