from mpl_toolkits.mplot3d import Axes3D
import matplotlib.pyplot as plt
import pandas as pd
import os

def invoke(file:str,
           delimiter:str=None,
           has_header:bool=None,
           column_names:str=None,
           column_nums:str=None,
           normalizer:str=None,
           metric:str=None,
           k:int=None,
           max_neighbour:int=None,
           num_local:int=None):

    cmd = 'dotnet dist/Groupping.NET.dll -f %s' % file
    sfx = ''

    if delimiter is not None:
        cmd = '%s -d %s' % (cmd, delimiter)
    
    if has_header is not None:
        cmd = '%s -h %s' % (cmd, 'True' if has_header else 'False')

    if column_names is not None:
        cmd = '%s -c %s' % (cmd, column_names)
        sfx = '%s[column_names:%s]' % (sfx, column_names)

    if column_nums is not None:
        cmd = '%s -i %s' % (cmd, column_nums)
        sfx = '%s[column_nums:%s]' % (sfx, column_nums)

    if normalizer is not None:
        cmd = '%s -z %s' % (cmd, normalizer)
        sfx = '%s[normalizer:%s]' % (sfx, normalizer)

    if metric is not None:
        cmd = '%s -m %s' % (cmd, metric)
        sfx = '%s[metric:%s]' % (sfx, metric)

    if k is not None:
        cmd = '%s -k %s' % (cmd, k)
        sfx = '%s[k:%s]' % (sfx, k)

    if max_neighbour is not None:
        cmd = '%s -n %s' % (cmd, max_neighbour)
        sfx = '%s[max_neighbour:%s]' % (sfx, max_neighbour)

    if num_local is not None:
        cmd = '%s -l %s' % (cmd, num_local)
        sfx = '%s[num_local:%s]' % (sfx, num_local)

    out_file = '%s.%s.out' % (file, sfx)
    cmd = '%s -o %s' % (cmd, out_file)

    os.system(cmd)

    return _concat_df(file, out_file)

def vis_3d(df: pd.DataFrame, cols):
    fig = plt.figure()
    ax = fig.add_subplot(111, projection='3d')

    ax.scatter(xs=df[cols[0]], ys=df[cols[1]], zs=df[cols[2]], c=df['ClusterNo'])
    ax.set_xlabel(cols[0])
    ax.set_ylabel(cols[1])
    ax.set_zlabel(cols[2])

    plt.show()

def vis_2d(df: pd.DataFrame, cols):
    plt.scatter(x=df[cols[0]], y=df[cols[1]], c=df['ClusterNo'])
    plt.xlabel(cols[0])
    plt.ylabel(cols[1])
    
    plt.show()

def _concat_df(csv_in: str, csv_out: str):
    df_in = pd.read_csv(csv_in)
    df_out = pd.read_csv(csv_out)
    return pd.concat([df_in, df_out], axis=1, join_axes=[df_in.index])
