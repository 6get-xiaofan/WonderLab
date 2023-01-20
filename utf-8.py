# 批量修改txt的编码方式为utf-8
# 注意，该.py文件需放在txt文件所在文件夹里运行才可以

import os
from chardet import detect
def recursion_dir_all_file(path):
    '''
    :param path: 文件夹目录
    '''
    file_list = []
    for dir_path, dirs, files in os.walk(path):
        for file in files:
            file_path = os.path.join(dir_path, file)
            if "\\" in file_path:
                file_path = file_path.replace('\\', '/')
            file_list.append(file_path)
        for dir in dirs:
            file_list.extend(recursion_dir_all_file(os.path.join(dir_path, dir)))
    return file_list

fileSuffixs = ["cs", "json", "txt", "axaml", "csproj"]
fns = []
filedir = os.path.join(os.path.abspath('.'), "")
    # os.path.abspath() 获取指定文件或目录的绝对路径
file_name = recursion_dir_all_file("C:\\Users\\Ddggd\\Desktop\\tmp\\WonderLab")
    # os.listdir() 用于返回一个由文件名和目录名组成的列表，即返回当前路径（文件夹）下所有文件的绝对路径列表
for fn in file_name:
    for fileSuffix in fileSuffixs:
        if fn.endswith(fileSuffix):
            # endswith() 判断字符串是否以指定后缀结尾
            fns.append(os.path.join(filedir, fn))
for fn in fns:
    with open(fn, 'rb+') as fp:
        content = fp.read()
        if len(content)==0:
            continue
        else:
            codeType = detect(content)['encoding']
            content = content.decode(codeType, "ignore").encode("utf8")
            fp.seek(0)
            fp.write(content)
            print(fn, "：已修改为utf8编码")
