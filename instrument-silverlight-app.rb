path = 'e:\projects\bcg\imd\src\imdclient\imdclient.web\clientbin'
#system("rename #{path}\\imdclient.xap imdclient.zip")

require 'FileUtils'
FileUtils.rm_rf 'temp'
Dir.mkdir('temp')

system("unzip #{path}\\imdclient.xap -d temp")
system('copy /Y NMetrics\bin\Debug\SilverlightProfiler.dll temp')
system('copy /Y NMetrics\bin\Debug\afterModification\imd\IMDClient.dll temp')
Dir.chdir 'temp'

system("del #{path}\\imdclient.xap")
system("zip #{path}\\imdclient.xap *.*")