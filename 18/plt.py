import matplotlib.pyplot as plt
import numpy as np

# Id-Vd
data = np.genfromtxt("./S1.TXT", skip_header=5)
plt.plot(data[:, 2], data[:, 4], label="")

plt.title(r"$I_d-V_d$")
plt.xlabel(r"$V_d$")
plt.ylabel(r"$I_d$")
plt.xlim((0.0, -2.0))
plt.ylim((0.0, -3.5e-5))

plt.savefig("Id_Vd.png")
plt.clf()


def get_top_vg(vec1, vec2):
    # print(vec_x, vec_y)
    max = 0
    max_i = 0
    for i, v in enumerate(vec2):
        if v < max:
            max = v
            max_i = i
    # print(vec_x[max_i], max_i)
    return vec1[max_i], max_i


def get_min(vec):
    min_n = 0
    for v in vec:
        min_n = min(min_n, v)
    return min_n


def plot_Id_Vg(data, out_file):
    n = len(data)
    n_half = int(n/2)
    diff_data = np.diff(data, axis=0)[0:n-1]
    Id = data[:, 3][0:n_half-1]
    Vg = data[:, 1][0:n_half-1]
    dId = diff_data[:, 3][0:n_half-1]
    dVg = diff_data[:, 1][0:n_half-1]
    gm = -dId/dVg

    plt.plot(Vg, Id, label=r"$I_d$")
    plt.plot(Vg, gm, label=r"$gm$")
    # 接線の計算
    vg_top, vg_top_i = get_top_vg(Vg, dId)
    gm_top = gm[vg_top_i]
    id_top = Id[vg_top_i]
    plt.vlines(x=vg_top, ymin=-10e-5, ymax=0,
               linestyles='dashed', colors='Black', linewidth=0.3)

    x = np.linspace(-2, 0, 100)
    y = [-gm_top*(i-vg_top)+id_top for i in x]
    Vt = vg_top+id_top/gm_top
    print("Vt = {}".format(Vt))
    plt.plot(x, y, linewidth=0.5, linestyle='dashed')

    plt.title(r"$I_d-V_g$")
    plt.xlabel(r"$V_g$")
    plt.ylabel(r"$I_d$")
    plt.xlim((0.0, -2.0))
    plt.ylim((0.0, get_min(gm)*1.2))

    plt.legend()
    plt.savefig(out_file)
    plt.clf()

    #移動度と実効垂直電界
    W = 100e-6
    L = 100e-6
    epsilon_si = 11.68
    Cox = 3.9*8.85e-12/1e-9
    Vd = data[:, 2][0:n_half-1]
    myu_eff = -L*Id/(W*Cox*(Vg - Vt)*Vd)

    eta = 1/3
    E_eff = -eta*Cox*(Vg - Vt)/epsilon_si
    plt.plot(E_eff, myu_eff)
    print(myu_eff)
    plt.title(r"$\mu_{eff}-I_d$")
    plt.xlabel(r"$e_{eff}$")
    plt.ylabel(r"$\mu_{eff}$")
    plt.xscale("log")
    plt.yscale("log")
    
    
    plt.savefig(out_file + '_mu')
    plt.clf()


# Id-Vg
data = np.genfromtxt("./S2.TXT", skip_header=5)
data_50m = data[0:202]
data_500m = data[202:404]

plot_Id_Vg(data_50m, "Id_Vg50m")
plot_Id_Vg(data_500m, "Id_Vg500m")
