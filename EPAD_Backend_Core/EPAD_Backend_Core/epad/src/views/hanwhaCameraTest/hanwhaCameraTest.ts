import { Component, Vue, Mixins, Watch } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import axios from 'axios';
import https from 'https';
import crypto from 'crypto';
import { apilink_AddEmployeeTranfer } from "@/constant/variable";

@Component({
  name: "hanwhaCameraTest",
  components: { HeaderComponent }
})
export default class HanwhaCameraTestComponent extends Mixins(ComponentBase) {
  streamSrc = "";
  resCamera: any = "";
  selectingCamera: any = "";
  stopStream = true;

  username = 'admin';
  password = 'Tinhhoasolutions!@#123';
  cameraId = '0578310b-5c7d-1f90-cfce-fc6252b2fdbe';
  systemId = 'a88de4f2-2b08-40f4-92a6-4167b396f3f0';
  serverCloudAddress = "https://a88de4f2-2b08-40f4-92a6-4167b396f3f0.relay.vmsproxy.com";
  serverLocalAddress = "https://103.98.160.202:7001";
  serverAddress = this.serverCloudAddress; // or serverLocalAddress;

  token = '';

  SOI = new Uint8Array(2);
  CONTENT_LENGTH = "Content-length";
  TYPE_JPEG = "image/jpeg";

  @Watch("selectingCamera")
  onChangeCamera(){
    // console.log(this.selectingCamera)
    if(this.selectingCamera && this.selectingCamera != ""){
      this.stopStream = false;
      this.turnOnStream();
    }else{
      this.stopStream = true;
    }
    // console.log(this.stopStream);
  }

  turnOnStream(){
    this.SOI[0] = 0xff;
    this.SOI[1] = 0xd8;
    const headers = new Headers();
    headers.append('Authorization', `Bearer ${this.token}`);
    // API stream không thể gọi bằng axios GET thông thường, phải dùng fetch để lấy ra kết quả liên tục 
    // và render dưới dạng ảnh jpeg
    // Note: không thể gán API stream trực tiếp cho thuộc tính "src" của thẻ image hoặc video 
    // vì không thể set trực tiếp header chứa bearer token authorization 
    const controller = new AbortController();
    const signal = controller.signal;
    fetch('https://a88de4f2-2b08-40f4-92a6-4167b396f3f0.relay.vmsproxy.com/media/' + this.selectingCamera + '.mpjpeg', {
      headers, signal
    }).then((res) => {
      // // console.log(res)
      let headers = "";
      let contentLength = -1;
      let imageBuffer = null;
      let bytesRead = 0;

      // calculating fps. This is pretty lame. Should probably implement a floating window function.
      let frames = 0;

      setInterval(() => {
        // // console.log("fps : " + frames);
        frames = 0;
      }, 1000);

      const reader = res.body.getReader();

      let read = () => {
        reader.read().then(({done, value}) => {
          // // console.log(value);
          if(done){
            // // console.log("end");
            return;
          }

          for (let index = 0; index < value.length; index++) {
            // we've found start of the frame. Everything we've read till now is the header.
            if (value[index] === this.SOI[0] && value[index + 1] === this.SOI[1]) {
              // // console.log('header found : ' + newHeader);
              contentLength = this.getLength(headers);
              // // console.log("Content Length : " + newContentLength);
              imageBuffer = new Uint8Array(
                new ArrayBuffer(contentLength)
              );
            }
            // we're still reading the header.
            if (contentLength <= 0) {
              headers += String.fromCharCode(value[index]);
            }
            // we're now reading the jpeg.
            else if (bytesRead < contentLength) {
              imageBuffer[bytesRead++] = value[index];
            }
            // we're done reading the jpeg. Time to render it.
            else {
              // // console.log("jpeg read with bytes : " + bytesRead);
              // document.getElementById("motionjpeg").setAttribute('src',URL.createObjectURL(
              //   new Blob([imageBuffer], { type: this.TYPE_JPEG })
              // ));
              this.streamSrc = URL.createObjectURL(
                new Blob([imageBuffer], { type: this.TYPE_JPEG })
              );
              frames++;
              contentLength = 0;
              bytesRead = 0;
              headers = "";
            }
          }
          if(this.stopStream){
            // document.getElementById("motionjpeg").setAttribute('src', "");
            this.streamSrc = "";
            controller.abort();
          }else{
            read();
          }
        });
      };
      if(this.stopStream){
        // document.getElementById("motionjpeg").setAttribute('src', "");
        this.streamSrc = "";
        controller.abort();
      }else{
        read();
      }
    });
  }

  unzoomStreamWisenetClient(){
    let instance = axios.create({
      httpsAgent: new https.Agent({  
        rejectUnauthorized: false,
      })
    });

    instance.defaults.headers.common['Authorization'] = `Bearer ${this.token}`;

    instance.get('https://a88de4f2-2b08-40f4-92a6-4167b396f3f0.relay.vmsproxy.com/rest/v2/layouts')
    .then((devicesRes: any) => {
      // console.log(devicesRes)
      // console.log(devicesRes.data)
      if(devicesRes.data && devicesRes.data.length > 0){
        devicesRes.data.forEach(element => {
          const layoutId = element.id;
          if(element.items && element.items.length > 0){
            element.items.forEach(item => {
              instance.patch('https://a88de4f2-2b08-40f4-92a6-4167b396f3f0.relay.vmsproxy.com/rest/v2/layouts/' 
                + layoutId + '/items/' + item.id,
              {
                // "flags": 1,
                // "bottom": 1,
                // "right": 2,
                // "left": 1,
                // "top" : 0,
                "resourceId": this.selectingCamera,
                "zoomLeft": 0,
                "zoomRight": 0,
                "zoomTop": 0,
                "zoomBottom": 0,
                // "zoomBottom": 0,
                // "zoomLeft": 0,
                // "zoomRight": 0,
                // "zoomTop": 0,
                // "zoomTargetId": "{a2493926-f0c3-4b11-845c-46ff5679d1a1}",
              }
              )
              .then((layoutRes: any) => {
                // console.log(layoutRes)
              }).catch((err) => {
                // console.log(err)
                // console.log(err.response)
              });
            });
          }
        });
      }
      this.resCamera = devicesRes;
    }).catch((err) => {
      // console.log(err)
      // console.log(err.response)
    });
  }

  zoomStreamWisenetClient(){
    let instance = axios.create({
      httpsAgent: new https.Agent({  
        rejectUnauthorized: false,
      })
    });

    instance.defaults.headers.common['Authorization'] = `Bearer ${this.token}`;

    instance.get('https://a88de4f2-2b08-40f4-92a6-4167b396f3f0.relay.vmsproxy.com/rest/v2/layouts')
    .then(async (devicesRes: any) => {
      // console.log(devicesRes)
      // console.log(devicesRes.data)
      if(devicesRes.data && devicesRes.data.length > 0){
        await devicesRes.data.forEach(async (element) => {
          const layoutId = element.id;
          if(element.items && element.items.length > 0){
            if(element.items.length > 1){
              for(let i = 0; i < element.items.length; i++) {    
                const item = element.items[i];
                if(i < (element.items.length - 1)){
                  try {  
                    const response = await instance.delete('https://a88de4f2-2b08-40f4-92a6-4167b396f3f0.relay.vmsproxy.com/rest/v2/layouts/' 
                      + layoutId + '/items/' + item.id)
                    .then((deleteLayoutItemRes: any) => {
                      // console.log(deleteLayoutItemRes)
                    })
                    .catch((err: any) => {
                      // console.log(err)
                      // console.log(err.response)
                    });
                  } catch (error) {
                    // console.log(error)
                  }
                }else{
                  try {  
                    const response = await instance.patch('https://a88de4f2-2b08-40f4-92a6-4167b396f3f0.relay.vmsproxy.com/rest/v2/layouts/' 
                      + layoutId + '/items/' + item.id,
                    {
                      "flags": 1,
                      "left": 0,
                      "top": 0,
                      "right": 1,
                      "bottom": 1,
                      "resourceId": this.selectingCamera,
                      "zoomLeft": 0.25,
                      "zoomRight": 0.75,
                      "zoomTop": 0.25,
                      "zoomBottom": 0.75,
                      // "zoomBottom": 0,
                      // "zoomLeft": 0,
                      // "zoomRight": 0,
                      // "zoomTop": 0,
                      // "zoomTargetId": "{a2493926-f0c3-4b11-845c-46ff5679d1a1}",
                    }
                    )
                    .then((layoutRes: any) => {
                      // console.log(layoutRes)
                    }).catch((err) => {
                      // console.log(err)
                      // console.log(err.response)
                    });
                  } catch (error) {
                    // console.log(error)
                  }
                }
              }
            }else{
              element.items.forEach(item => {
                instance.patch('https://a88de4f2-2b08-40f4-92a6-4167b396f3f0.relay.vmsproxy.com/rest/v2/layouts/' 
                  + layoutId + '/items/' + item.id,
                {
                  "flags": 1,
                  "left": 0,
                  "top": 0,
                  "right": 1,
                  "bottom": 1,
                  "resourceId": this.selectingCamera,
                  "zoomLeft": 0.25,
                  "zoomRight": 0.75,
                  "zoomTop": 0.25,
                  "zoomBottom": 0.75,
                  // "zoomBottom": 0,
                  // "zoomLeft": 0,
                  // "zoomRight": 0,
                  // "zoomTop": 0,
                  // "zoomTargetId": "{a2493926-f0c3-4b11-845c-46ff5679d1a1}",
                }
                )
                .then((layoutRes: any) => {
                  // console.log(layoutRes)
                }).catch((err) => {
                  // console.log(err)
                  // console.log(err.response)
                });
              });
            }
          }
        });
      }
      this.resCamera = devicesRes;
    }).catch((err) => {
      // console.log(err)
      // console.log(err.response)
    });
  }

  beforeMount(){
    // Luôn gọi API này trước tiên để lấy token và set cookie bằng token
    // // 'https://192.168.1.48:7001/rest/v2/login/sessions'
    // axios.post('https://a88de4f2-2b08-40f4-92a6-4167b396f3f0.relay.vmsproxy.com/rest/v2/login/sessions', {
    axios.post('https://a88de4f2-2b08-40f4-92a6-4167b396f3f0.relay.vmsproxy.com/rest/v2/login/sessions', {
      "username": this.username,
      "password": this.password,
      "setCookie": true,
    }).then(async (res: any) => {
      if(res && res.status && res.status == 200){
        // // console.log(res);

        let instance = axios.create({
          httpsAgent: new https.Agent({  
            rejectUnauthorized: false,
          })
        });

        // // console.log("set cookie")
        this.token = res.data.token;
        document.cookie = "x-runtime-guid=" + this.token;
        // // console.log(document.cookie)

        // Dùng token để tạo authorization trong header, sử dụng trong các API khác
        // instance.defaults.withCredentials = true;
        instance.defaults.headers.common['Authorization'] = `Bearer ${this.token}`;

        instance.get('https://a88de4f2-2b08-40f4-92a6-4167b396f3f0.relay.vmsproxy.com/rest/v2/devices')
        .then((devicesRes: any) => {
          if(devicesRes && devicesRes.data){
            this.resCamera = devicesRes.data;
            // console.log(this.resCamera)
          }
        }).catch((err) => {
          // console.log(err)
          // console.log(err.response)
        });

        // instance.get('https://a88de4f2-2b08-40f4-92a6-4167b396f3f0.relay.vmsproxy.com/rest/v2/layouts')
        // .then((devicesRes: any) => {
        //   // console.log(devicesRes)
        //   // console.log(devicesRes.data)
        //   if(devicesRes.data && devicesRes.data.length > 0){
        //     devicesRes.data.forEach(element => {
        //       const layoutId = element.id;
        //       if(element.items && element.items.length > 0){
        //         element.items.forEach(item => {
        //           // instance.delete('https://a88de4f2-2b08-40f4-92a6-4167b396f3f0.relay.vmsproxy.com/rest/v2/layouts/' 
        //           //   + layoutId + '/items/' + item.id)
        //           // .then((deleteLayoutItemRes: any) => {
        //           //   // console.log(deleteLayoutItemRes)
        //           // })
        //           // .catch((err: any) => {
        //           //   // console.log(err)
        //           //   // console.log(err.response)
        //           // });
        //         });
        //       }
        //     });
        //   }
        //   this.resCamera = devicesRes;
        // }).catch((err) => {
        //   // console.log(err)
        //   // console.log(err.response)
        // });

        // //ADD ITEM TO LAYOUT
        // instance.post('https://a88de4f2-2b08-40f4-92a6-4167b396f3f0.relay.vmsproxy.com/rest/v2/layouts/' 
        //   + 'be1c41ee-5c4f-4ee3-a4dc-3d61467051f2/items',
        // {
        //   "flags": 1,
        //   "left": 0,
        //   "top": 0,
        //   "right": 1,
        //   "bottom": 1,
        //   "resourceId": "0578310b-5c7d-1f90-cfce-fc6252b2fdbe",
        //   // "zoomLeft": 0,
        //   // "zoomTop": 0,
        //   // "zoomRight": 0,
        //   // "zoomBottom": 0,
        // }
        // )
        // .then((layoutRes: any) => {
        //   // console.log(layoutRes)
        // }).catch((err) => {
        //   // console.log(err)
        //   // console.log(err.response)
        // });

        // // UPDATE ITEM IN LAYOUT
        // instance.patch('https://a88de4f2-2b08-40f4-92a6-4167b396f3f0.relay.vmsproxy.com/rest/v2/layouts/' 
        //   + 'be1c41ee-5c4f-4ee3-a4dc-3d61467051f2/items/d8622cba-b20a-46e9-ab0b-358f5ccce3a4',
        // {
        //   "flags": 1,
        //   "bottom": 1,
        //   "right": 2,
        //   "left": 1,
        //   "top" : 0,
        //   "resourceId": "{0578310b-5c7d-1f90-cfce-fc6252b2fdbe}",
        //   "zoomBottom": 1,
        //   "zoomLeft": 0.5,
        //   "zoomRight": 1,
        //   "zoomTop": 0.5,
        // //   "zoomBottom": 0,
        // //   "zoomLeft": 0,
        // //   "zoomRight": 0,
        // //   "zoomTop": 0,
        //   // "zoomTargetId": "{a2493926-f0c3-4b11-845c-46ff5679d1a1}",
        // }
        // )
        // .then((layoutRes: any) => {
        //   // console.log(layoutRes)
        // }).catch((err) => {
        //   // console.log(err)
        //   // console.log(err.response)
        // });



        // this.SOI[0] = 0xff;
        // this.SOI[1] = 0xd8;
        // const headers = new Headers();
        // headers.append('Authorization', `Bearer ${res.data.token}`);
        // // API stream không thể gọi bằng axios GET thông thường, phải dùng fetch để lấy ra kết quả liên tục 
        // // và render dưới dạng ảnh jpeg
        // // Note: không thể gán API stream trực tiếp cho thuộc tính "src" của thẻ image hoặc video 
        // // vì không thể set trực tiếp header chứa bearer token authorization 
        // fetch('https://a88de4f2-2b08-40f4-92a6-4167b396f3f0.relay.vmsproxy.com/media/0578310b-5c7d-1f90-cfce-fc6252b2fdbe.mpjpeg', {
        //   headers 
        // }).then((res) => {
        //   let headers = "";
        //   let contentLength = -1;
        //   let imageBuffer = null;
        //   let bytesRead = 0;

        //   // calculating fps. This is pretty lame. Should probably implement a floating window function.
        //   let frames = 0;

        //   setInterval(() => {
        //     // // console.log("fps : " + frames);
        //     frames = 0;
        //   }, 1000);

        //   const reader = res.body.getReader();

        //   const read = () => {
        //     reader.read().then(({done, value}) => {
        //       // // console.log(value);
        //       if(done){
        //         // // console.log("end");
        //         return;
        //       }

        //       for (let index = 0; index < value.length; index++) {
        //         // we've found start of the frame. Everything we've read till now is the header.
        //         if (value[index] === this.SOI[0] && value[index + 1] === this.SOI[1]) {
        //           // // console.log('header found : ' + newHeader);
        //           contentLength = this.getLength(headers);
        //           // // console.log("Content Length : " + newContentLength);
        //           imageBuffer = new Uint8Array(
        //             new ArrayBuffer(contentLength)
        //           );
        //         }
        //         // we're still reading the header.
        //         if (contentLength <= 0) {
        //           headers += String.fromCharCode(value[index]);
        //         }
        //         // we're now reading the jpeg.
        //         else if (bytesRead < contentLength) {
        //           imageBuffer[bytesRead++] = value[index];
        //         }
        //         // we're done reading the jpeg. Time to render it.
        //         else {
        //           // // console.log("jpeg read with bytes : " + bytesRead);
        //           document.getElementById("motionjpeg").setAttribute('src',URL.createObjectURL(
        //             new Blob([imageBuffer], { type: this.TYPE_JPEG })
        //           ));
        //           frames++;
        //           contentLength = 0;
        //           bytesRead = 0;
        //           headers = "";
        //         }
        //       }

        //       read();
        //     });
        //   };
        //   read();
        // });
      }
    }).catch((err) => {
      // console.log(err)
      // console.log(err.response)
    });
  }

  async mounted() {
    // // Call API without authentication to get header, use for generate digest authentication header
    // const deviceDigestAuthResult = await this.generateDigestAuthrorizeHeader(
    //   'http://192.168.1.29:8081',
    //   '/stw-cgi/system.cgi?msubmenu=deviceinfo&action=view',
    //   'admin',
    //   'Admin@123');
    // // console.log(deviceDigestAuthResult);
    // // Use above digest authentication for call any API which need digest authentication
    // if(deviceDigestAuthResult != ''){
    //   axios.get('http://192.168.1.29:8081/stw-cgi/system.cgi?msubmenu=deviceinfo&action=view', 
    //     { headers: { Authorization: deviceDigestAuthResult } })
    //   .then((res: any) => {
    //     // console.log(res)
    //   });
    // }
  }

  saveImage() {
    // Replace 'blobURL' with the actual Blob URL of the image
    const blobURL = document.getElementById("motionjpeg").getAttribute('src');
    // console.log(blobURL)

    // Create an XMLHttpRequest object
    const xhr = new XMLHttpRequest();

    // Open a GET request to the Blob URL
    xhr.open('GET', blobURL, true);

    // Set the responseType to 'blob' (binary data)
    xhr.responseType = 'blob';

    // Set up an event listener to handle the successful response
    xhr.onload = function() {
      if (xhr.status === 200) {
        // Create a new Blob object from the response data
        const blob = xhr.response;

        // Create a URL for the Blob
        const blobURL = URL.createObjectURL(blob);

        // Create an <a> element to trigger the download
        const a = document.createElement('a');
        a.href = blobURL;
        a.download = 'image.jpg'; // You can specify the desired file name
        document.body.appendChild(a);

        // Trigger a click event on the <a> element to initiate the download
        a.click();

        // Clean up by revoking the Blob URL
        URL.revokeObjectURL(blobURL);
      }
    };

    // Send the XMLHttpRequest
    xhr.send();
  }

  captureImage() {
    // Replace 'blobURL' with the actual Blob URL of the image
    const blobURL = document.getElementById("motionjpeg").getAttribute('src');
    // console.log(blobURL)

    document.getElementById("captureImage").setAttribute('src', blobURL);
  }

  getLength = (headers) => {
    let contentLength = -1;
    headers.split("\n").forEach((header, _) => {
      const pair = header.split(":");
      if (pair[0] === this.CONTENT_LENGTH) {
        contentLength = pair[1];
      }
    });
    return contentLength;
  };

  async generateDigestAuthrorizeHeader(base, api, username, password){
    let count = 0;
    let authorization = '';
    await axios.get(base + api)
    .catch(err => {
      // console.log(err.response)
      if (err.response.status == 401) {
        const authDetails = err.response.headers['www-authenticate'].split(', ').map(v => v.split('='));

        ++count;
        // const nonceCount = ('00000000' + count).slice(-8);
        const nonceCount: number = 1;
        const cnonce = crypto.randomBytes(24).toString('hex');
        const realm = authDetails[0][1].replace(/"/g, '');
        // console.log(realm)
        const nonce = authDetails[1][1].replace(/"/g, '');
        // console.log(nonce)

        const md5 = str => crypto.createHash('md5').update(str).digest('hex');
        const HA1 = md5(`${username}:${realm}:${password}`);
        const HA2 = md5(`GET:${api}`);
        const response = md5(`${HA1}:${nonce}:${nonceCount}:${cnonce}:auth:${HA2}`);
        authorization = `Digest username="${username}",realm="${realm}",` +
          `nonce="${nonce}",uri="${api}",qop="auth",algorithm="MD5",` +
          `response="${response}",nc="${nonceCount}",cnonce="${cnonce}"`;
      }
    });
    return authorization;
  }
}
