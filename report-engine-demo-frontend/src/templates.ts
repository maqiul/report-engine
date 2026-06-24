/**
 * 前端模板常量（与后端 Templates.java 保持一致）
 * 真实项目应从后端拉取
 */
export const Templates = {
  SALES_ORDER: JSON.stringify({
    version: '1.0',
    page: { width: 210, height: 297, margin: { top: 20, bottom: 20, left: 15, right: 15 } },
    dataSources: [{ name: 'orders', type: 'json' }],
    bands: [
      {
        type: 'title', height: 12,
        elements: [{
          type: 'text', text: '销售订单', x: 0, y: 0, width: 180, height: 10,
          font: { size: 20, bold: true }, alignment: 'center'
        }]
      },
      {
        type: 'detail', height: 80, dataSource: 'orders',
        elements: [
          { type: 'text', text: '订单号: {{orders.id}}',       x: 0,  y: 5,  width: 90, height: 6, font: { size: 12, bold: true } },
          { type: 'text', text: '客户: {{orders.customer}}',   x: 0,  y: 18, width: 90, height: 6 },
          { type: 'text', text: '产品: {{orders.product}}',     x: 0,  y: 31, width: 90, height: 6 },
          { type: 'text', text: '数量: {{orders.quantity}}',   x: 0,  y: 44, width: 45, height: 6 },
          { type: 'text', text: '单价: ¥{{orders.unitPrice}}', x: 45, y: 44, width: 45, height: 6 },
          { type: 'text', text: '日期: {{orders.orderDate}}',   x: 0,  y: 57, width: 90, height: 6 },
          { type: 'text', text: '合计: ¥{{orders.total}}',     x: 0,  y: 70, width: 90, height: 8,
            font: { size: 14, bold: true }, alignment: 'right' }
        ]
      }
    ]
  }),

  SALES_SUMMARY: JSON.stringify({
    version: '1.0',
    page: { width: 210, height: 297, margin: { top: 20, bottom: 20, left: 15, right: 15 } },
    dataSources: [{ name: 'orders', type: 'json' }],
    bands: [
      {
        type: 'title', height: 12,
        elements: [{
          type: 'text', text: '销售订单汇总', x: 0, y: 0, width: 180, height: 10,
          font: { size: 20, bold: true }, alignment: 'center'
        }]
      },
      {
        type: 'pageHeader', height: 8,
        elements: [
          { type: 'text', text: '订单号', x: 0,   y: 0, width: 30, height: 6, font: { bold: true } },
          { type: 'text', text: '客户',   x: 30,  y: 0, width: 40, height: 6, font: { bold: true } },
          { type: 'text', text: '产品',   x: 70,  y: 0, width: 40, height: 6, font: { bold: true } },
          { type: 'text', text: '数量',   x: 110, y: 0, width: 20, height: 6, font: { bold: true }, alignment: 'right' },
          { type: 'text', text: '单价',   x: 130, y: 0, width: 25, height: 6, font: { bold: true }, alignment: 'right' },
          { type: 'text', text: '合计',   x: 155, y: 0, width: 25, height: 6, font: { bold: true }, alignment: 'right' }
        ]
      },
      {
        type: 'detail', height: 7, dataSource: 'orders',
        elements: [
          { type: 'text', text: '{{orders.id}}',         x: 0,   y: 0, width: 30, height: 6 },
          { type: 'text', text: '{{orders.customer}}',   x: 30,  y: 0, width: 40, height: 6 },
          { type: 'text', text: '{{orders.product}}',    x: 70,  y: 0, width: 40, height: 6 },
          { type: 'text', text: '{{orders.quantity}}',   x: 110, y: 0, width: 20, height: 6, alignment: 'right' },
          { type: 'text', text: '¥{{orders.unitPrice}}', x: 130, y: 0, width: 25, height: 6, alignment: 'right' },
          { type: 'text', text: '¥{{orders.total}}',     x: 155, y: 0, width: 25, height: 6, alignment: 'right' }
        ]
      }
    ]
  })
}
