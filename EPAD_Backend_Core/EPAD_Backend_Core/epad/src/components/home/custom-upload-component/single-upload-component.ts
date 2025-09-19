import * as mime from 'mime-types';
import { Component, Model, Prop, Vue } from 'vue-property-decorator';
import { ezPortalFileApi, EzFile } from '@/$api/ez-portal-file-api';

@Component({
  name: 'single-upload-component',
})
export default class SingleUploadComponent extends Vue {
  @Model() model: EzFile[];
  @Prop({ default: 'upload' }) id: string;

  @Prop() accept: string;
  @Prop() confirmDelete: boolean;
  @Prop() multiple: boolean;
  @Prop({ default: 'ChooseFile' }) buttonName: string;
  @Prop({ default: 0 }) limitFiles: number;
  openFile(file: EzFile) {
    try {
      if (file.Url.startsWith('data:')) {
        window.open(
          URL.createObjectURL(Misc.dataURLtoFile(file.Url, file.Name)),
          '_blank'
        );
      } else {
        ezPortalFileApi.GetFile(file)
          .then((done) => {
            window.open(
              URL.createObjectURL(
                Misc.dataURLtoFile(
                  'data:' +
                  mime.lookup(file.Name) +
                  ';base64,' +
                  Misc.arrayBufferToBase64(done),
                  file.Name
                )
              ),
              '_blank'
            );
          })
          .catch(() =>
            this.$alertRequestError(null, null, this.$t('Error').toString(), this.$t("messages.can-not-open-file").toString()));
      }
    } catch {
      this.$alertRequestError(null, null, this.$t('Error').toString(), this.$t("messages.can-not-open-file").toString());
    }
  }

  downloadFile() {
    this.model.forEach(element => {
      const file: EzFile = element;
      // console.log("file", file, this.model);
      try {
        if (file.Url.startsWith('data:')) {
          Misc.saveBase64(file.Name, file.Url);
        } else {
          ezPortalFileApi.GetFile(file)
            .then((done) => Misc.saveByteArray(file.Name, done))
            .catch(() =>
              this.$alertRequestError(null, null, this.$t('Error').toString(), this.$t("messages.can-not-download-file").toString())
            );
        }
      } catch {
        this.$alertRequestError(null, null, this.$t('Error').toString(), this.$t("messages.can-not-download-file").toString())
      }
    });

  }

  deleteFile(file: EzFile) {
    if (this.confirmDelete && !file.Url.startsWith('data:')) {
      this.$confirm(
        this.$t('messages.delete-ezfile').toString(),
        this.$t('resources.confirm-delete-title').toString(),
        {
          type: 'warning',
        }
      ).then(() =>
        this.$emit(
          'model',
          this.model.filter((x) => x !== file)
        )
      );
    } else {
      this.$emit(
        'model',
        this.model.filter((x) => x !== file)
      );
    }
  }

  fileSelect(evt) {
    // console.log("fileSelect");
    if (this.validateLimitFile(evt.target.files)) {
      this.filesToEzFile(evt.target.files).then((rs) => {
        if (rs.length > 0) {
          // console.log("fileSelect model", evt.target.files, rs);
          this.$emit('model', (rs as EzFile[]));
        }
      });
    } else {
      this.$confirm(this.$t('NumberFilesMaximum') + ': ' + this.limitFiles, 'Warning', {
        confirmButtonText: 'OK',
        showCancelButton: false,
        type: 'warning',
        center: true
      })
    }

  }

  filesToEzFile(files: FileList) {
    let arr = Array.from(files);
    // console.log("model before", this.model, arr);
    if (this.accept) {
      arr = arr.filter((x) =>
        this.accept.split(';').includes(mime.lookup(x.name))
      );
      // console.log("model", this.model, arr);
      if (arr.length === 0) {
        this.$alertRequestError(null, null, this.$t('Error').toString(), this.$t("messages.no-valid-file").toString())
        throw new Error('no valid file was selected');
      }
    }
    const pms = arr.map(
      (x) =>
        new Promise((rs, rj) => {
          const reader = new FileReader();
          reader.onload = () => {
            const result: EzFile = {
              Name: x.name,
              Url: reader.result as string,
            };
            rs(result);
          };
          reader.onerror = (error) => rj(error);
          reader.readAsDataURL(x);
        })
    );
    return Promise.all(pms);
  }
  validateLimitFile(files: FileList) {
    const arr = Array.from(files);
    if (this.limitFiles != 0 && arr.length > this.limitFiles) {
      return false;
    }
    return true;
  }
  get getAccept() {
    return this.accept.split(';').join(',')
  }
  test() {
    document.getElementById(this.id).click();
  }
}


