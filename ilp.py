import gurobipy as gp
from gurobipy import GRB
import numpy as np 
from solve import read_instance
from seating import Seating
import math 


def make_ILP(filename='instances/mini.txt'):
    problem, people, rows, columns  = read_instance(filename)

    group_amount = int(np.sum([z for _,z in people.items()]))
    group_sizes = np.concatenate([np.array(n*[i+1], dtype=int) for i,n in people.items()]) 

    # g1 g2 g3
    # 1  2  1   
    people_amount = np.sum(group_sizes)
    print(group_sizes, group_amount, people_amount)

    # Instantiate a gurobi ILP model
    model = gp.Model();

    # Start position (Col, Row) for every group
    # (-1,-1) means a group is not seated
    # The maximum Col-position is columns-1 
    col = model.addVars(group_amount, lb=-1, ub=columns-1, vtype=GRB.INTEGER, name="col")
    row = model.addVars(group_amount, lb=-1, ub=rows-1, vtype=GRB.INTEGER, name="row")
    # Every group has a constant size  
    size  = model.addVars(group_amount, lb=tuple(group_sizes), ub=tuple(group_sizes), vtype=GRB.INTEGER, name="groupsize")

    # Binary indicators
    # TODO: If a group is not seated these constraints shouldnt even be activated!
    hor1 = model.addVars(group_amount, group_amount, vtype=GRB.BINARY, name="hor1")
    hor2 = model.addVars(group_amount, group_amount, vtype=GRB.BINARY, name="hor2")
    ver1 = model.addVars(group_amount, group_amount, vtype=GRB.BINARY, name="ver1")
    ver2 = model.addVars(group_amount, group_amount, vtype=GRB.BINARY, name="ver2")

    diff_hor = model.addVars(group_amount, group_amount, vtype=GRB.INTEGER, name="diff_hor")
    diff_ver = model.addVars(group_amount, group_amount, vtype=GRB.INTEGER, name="diff_ver")


    diag_guy1 =  model.addVars(group_amount, group_amount, vtype=GRB.INTEGER, name="diagguy1")
    diag_guy2 =  model.addVars(group_amount, group_amount, vtype=GRB.INTEGER, name="diagguy2")

    diag1 = model.addVars(group_amount, group_amount, vtype=GRB.BINARY, name="diag1")
    diag2 = model.addVars(group_amount, group_amount, vtype=GRB.BINARY, name="diag2")

    groupseated = model.addVars(group_amount, vtype=GRB.BINARY, name='groupseated')
    groupseatedx = model.addVars(group_amount, vtype=GRB.BINARY, name='groupseatedx')
    groupseatedy = model.addVars(group_amount, vtype=GRB.BINARY, name='groupseatedy')


    # Reward[i] holds the value seated[i] (0/1) * 
    reward = model.addVars(group_amount, vtype=GRB.INTEGER, name='reward')
    finalreward = model.addVar(lb=0, ub=people_amount, vtype=GRB.INTEGER, name='finalreward')


    for i in range(group_amount):
        # Any start_X position + group_size of that group should not "fall out of cinema"
        model.addConstr(col[i]+size[i], GRB.LESS_EQUAL, columns)

        # Whether a group is seated
        model.addGenConstrIndicator(groupseatedx[i], 1, col[i], GRB.GREATER_EQUAL, 0)
        model.addGenConstrIndicator(groupseatedy[i], 1, row[i], GRB.GREATER_EQUAL, 0)

        model.addGenConstrIndicator(groupseated[i], 1, groupseatedx[i] + groupseatedy[i], GRB.EQUAL, 2)

        # When a group is not seated, both should be -1 
        model.addConstr(groupseatedx[i], GRB.EQUAL, groupseatedy[i])


        model.addConstr(reward[i], GRB.EQUAL, groupseated[i] * size[i])

        
        # If sx = -1 then also sy = -1 
        
        # Do not sit in illegal places
        #model.addConstr()


        # Keep your distance
        for j in range(group_amount):
            if i != j:
                # Hor: Either si is to the right of sj by at least 3 spots
                model.addGenConstrIndicator(hor1[i,j], 1, col[i] - (col[j] + size[j]),  GRB.GREATER_EQUAL, 2)
                model.addGenConstrIndicator(hor2[i,j], 1, col[j] - col[i],  GRB.GREATER_EQUAL, 2)
                
                # Ver: Either si is two rows away from sj
                model.addGenConstrIndicator(ver1[i,j], 1, row[i] - row[j],  GRB.GREATER_EQUAL, 2)
                model.addGenConstrIndicator(ver2[i,j], 1, row[j] - row[i],  GRB.GREATER_EQUAL, 2)

                # diff
                model.addConstr(diff_hor[i,j] == row[i] - row[j])
                model.addConstr(diff_ver[i,j] ==  (col[i]+size[i]+1) - col[j])


                # Diag 
                model.addGenConstrAbs(diag_guy1[i,j], diff_hor[i,j])
                model.addGenConstrAbs(diag_guy2[i,j], diff_ver[i,j])


                model.addGenConstrIndicator(diag1[i,j], 1, diag_guy1[i,j] - diag_guy2[i,j] , GRB.GREATER_EQUAL, 1)
                model.addGenConstrIndicator(diag2[i,j], 1, diag_guy2[i,j] - diag_guy1[i,j]  ,  GRB.GREATER_EQUAL, 1)
                

                # TODO: Add "Groupsize * Reward" or something
                
                model.addConstr(hor1[i,j]+hor2[i,j]+ver1[i,j]+ver2[i,j]+diag1[i,j]+diag2[i,j], GRB.GREATER_EQUAL, 1)
                

    model.addConstr(finalreward, GRB.EQUAL, gp.quicksum(reward))
    model.setObjective(finalreward, GRB.MAXIMIZE);    


    # model.relax()
    model.optimize();
    solution = problem.copy()
    for i in range(group_amount):
        posx = int(col[i].x)
        posy = int(row[i].x )
        grp = int(size[i].x) 
        print("Position (", posx, ", ", posy, ") for group of size:",  grp)
        #print(test[posx,:])
        #break
        if posx>=0 and posy>=0:
            solution[posy, posx:posx+grp] = np.zeros(grp) +2 
    print(solution)

make_ILP()