import {
    AlignmentType,
    Document,
    HeadingLevel,
    Packer,
    Paragraph,
    TabStopPosition,
    TabStopType,
    TextRun, Table, TableRow, TableCell, BorderStyle, UnderlineType
} from "docx";
import { single } from "rxjs";
const PHONE_NUMBER = "07534563401";
const PROFILE_URL = "https://www.linkedin.com/in/dolan1";
const EMAIL = "docx@docx.com";

export class DocumentService {
    public create(): Document {
        const document = new Document({
            sections: [
                {
                    children: [
                        this.createTable()
                    ]
                }
            ]
        });

        return document;
    }

    public createContactInfo(
        phoneNumber: string,
        profileUrl: string,
        email: string
    ): Paragraph {
        return new Paragraph({
            alignment: AlignmentType.CENTER,
            children: [
                new TextRun(
                    `Mobile: ${phoneNumber} | LinkedIn: ${profileUrl} | Email: ${email}`
                ),
                new TextRun({
                    text: "Address: 58 Elm Avenue, Kent ME4 6ER, UK",
                    break: 1
                })
            ]
        });
    }

    public createHeading(text: string): Paragraph {
        return new Paragraph({
            text: text,
            heading: HeadingLevel.HEADING_1,
            thematicBreak: true
        });
    }

    public createSubHeading(text: string): Paragraph {
        return new Paragraph({
            text: text,
            heading: HeadingLevel.HEADING_2
        });
    }

    public createInstitutionHeader(
        institutionName: string,
        dateText: string
    ): Paragraph {
        return new Paragraph({
            tabStops: [
                {
                    type: TabStopType.RIGHT,
                    position: TabStopPosition.MAX
                }
            ],
            children: [
                new TextRun({
                    text: institutionName,
                    bold: true
                }),
                new TextRun({
                    text: `\t${dateText}`,
                    bold: true
                })
            ]
        });
    }

    public createRoleText(roleText: string): Paragraph {
        return new Paragraph({
            children: [
                new TextRun({
                    text: roleText,
                    italics: true
                })
            ]
        });
    }

    public createBullet(text: string): Paragraph {
        return new Paragraph({
            text: text,
            bullet: {
                level: 0
            }
        });
    }

    // tslint:disable-next-line:no-any
    public createSkillList(skills: any[]): Paragraph {
        return new Paragraph({
            children: [new TextRun(skills.map(skill => skill.name).join(", ") + ".")]
        });
    }

    // tslint:disable-next-line:no-any
    public createAchivementsList(achivements: any[]): Paragraph[] {
        return achivements.map(
            achievement =>
                new Paragraph({
                    text: achievement.name,
                    bullet: {
                        level: 0
                    }
                })
        );
    }

    public createInterests(interests: string): Paragraph {
        return new Paragraph({
            children: [new TextRun(interests)]
        });
    }

    public splitParagraphIntoBullets(text: string): string[] {
        return text.split("\n\n");
    }

    // tslint:disable-next-line:no-any
    public createPositionDateText(
        startDate: any,
        endDate: any,
        isCurrent: boolean
    ): string {
        const startDateText =
            this.getMonthFromInt(startDate.month) + ". " + startDate.year;
        const endDateText = isCurrent
            ? "Present"
            : `${this.getMonthFromInt(endDate.month)}. ${endDate.year}`;

        return `${startDateText} - ${endDateText}`;
    }

    public getMonthFromInt(value: number): string {
        switch (value) {
            case 1:
                return "Jan";
            case 2:
                return "Feb";
            case 3:
                return "Mar";
            case 4:
                return "Apr";
            case 5:
                return "May";
            case 6:
                return "Jun";
            case 7:
                return "Jul";
            case 8:
                return "Aug";
            case 9:
                return "Sept";
            case 10:
                return "Oct";
            case 11:
                return "Nov";
            case 12:
                return "Dec";
            default:
                return "N/A";
        }
    }

    // Hàm tự define 


    public createTable() {
        return new Table({
            rows: [
                new TableRow({
                    children: [
                        new TableCell({
                            children: this.createLeftHeaderTable()
                        })
                    ],
                })
            ], borders: {
                top: { style: BorderStyle.NONE, size: 0, color: "FFFFFF" },
                bottom: { style: BorderStyle.NONE, size: 0, color: "FFFFFF" },
                left: { style: BorderStyle.NONE, size: 0, color: "FFFFFF" },
                right: { style: BorderStyle.NONE, size: 0, color: "FFFFFF" }
            }
        });
    }

    public createLeftHeaderTable(): Paragraph[] {
        return [
            new Paragraph({
                children: [
                    new TextRun({
                        text: "PHÒNG QUẢN LÝ NHÀ Ở",
                        size: 25,
                        font: 'Times New Roman'
                    })
                ]
            }),
            new Paragraph({
                children: [
                    // new HorizontalLine()
                    new TextRun({
                        underline: {
                            type: UnderlineType.SINGLE,
                            color: "#cccccc"
                        }
                    })
                ],
                // thematicBreak: true,
                // spacing: {
                //     before: 100,
                //     after: 80
                // },
                indent: { left: 720, right: 720 }
            }),
            new Paragraph({
                children: [
                    new TextRun({
                        text: "TỔ KỸ THUẬT-TÍNH GIÁ",
                        size: 25,
                        font: 'Times New Roman',
                        bold: true,
                        // break: 1
                    })
                ]
            })
        ];
    }
}
