const { location } = window;
const { hostname } = location || {};

//#region  --- Không sửa ----
const url = 'http://noc.cnttvietnam.com.vn';
//const url = 'https://localhost:44322';
// const url = 'https://localhost:44345';
//#endregion

switch (hostname) {
  // case 'invoice-test.bluesea.vsn':
  //     url = 'http://gateway-test.bluesea.vn/einvoice/v1';
  //     break;
  case 'localhost':
    // url = 'http://10.145.20.35:8082/ca_saas/v1';
    //     break;

    break;
}

export default {
  // urlRefreshToken,
  api_gateway: url,
  domain_img: url + '/uploads/',
  domain_thumb_img: url + '/uploads/thumbs/thumb500/',
  api_upload_img: url + '/api/Upload/uploadImage'
};
