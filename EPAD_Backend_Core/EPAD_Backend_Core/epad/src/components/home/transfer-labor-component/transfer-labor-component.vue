<template>
  <div>
    <el-row>
      <el-col :span="5" class="Tree">
        <el-tree
          :data="data"
          show-checkbox
          default-expand-all
          node-key="id"
          ref="tree"
          highlight-current
        ></el-tree>
      </el-col>
      <el-col :span="19" class="DepartmentTreeComponent">
        <div>
          <el-container>
            <el-header>
              <el-row>
                <el-col :span="12" class="left">
                  <span id="FormName">{{Title}}</span>
                </el-col>
                <el-col :span="12">
                  <HeaderComponent />
                </el-col>
              </el-row>
            </el-header>
            <el-main>
              <el-row>
                <el-col :span="6" id="col-1">
                  <el-form>
                    <el-form-item>
                      <span>{{FromDate}}</span>
                      <el-date-picker type="date" placeholder="Pick a day"></el-date-picker>
                    </el-form-item>
                  </el-form>
                </el-col>
                <el-col :span="6" id="col-2">
                  <el-form>
                    <el-form-item>
                      <span>{{FromDate}}</span>
                      <el-date-picker type="date" placeholder="Pick a day"></el-date-picker>
                    </el-form-item>
                  </el-form>
                </el-col>
                <el-col :span="1" id="col-3">
                  <el-button round class="btnView">{{ BtnView }}</el-button>
                </el-col>
                <el-col :span="9" class="colText">
                  <a href="#">
                    <span class="spanSchedule">{{Schedule}}</span>
                  </a>
                </el-col>
              </el-row>
              <el-row>
                <el-col :span="24">
                  <el-table
                    :data="tableData"
                    :default-sort="{prop: 'date', order: 'descending'}"
                    style="width: 100%"
                  >
                    <el-table-column prop="date" label="Date" sortable width="180"></el-table-column>
                    <el-table-column prop="name" label="Name" width="180"></el-table-column>
                    <el-table-column prop="address" label="Address" ></el-table-column>
                  </el-table>
                </el-col>
              </el-row>
            </el-main>
          </el-container>
        </div>
      </el-col>
    </el-row>
  </div>
</template>

<script src="./transfer-labor-component.ts">
</script>
